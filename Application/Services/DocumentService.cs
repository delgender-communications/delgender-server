using Core.Configuration;
using Core.DTOs.Documents;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using System.Collections.Concurrent;

namespace Application.Services
{
    public class DocumentService : IDocumentService
    {
        // Email providers cap the total size of attachments, so stay well under it.
        private const long MaxEmailBytes = 30_000_000;

        private readonly IDocumentRepository _documentRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IPitchDeckFetcher _fileFetcher;
        private readonly IDocumentMailer _documentMailer;

        // Clients whose documents are being sent right now. Stops a double click (or two
        // teammates clicking at once) from emailing the same client twice.
        private static readonly ConcurrentDictionary<int, byte> SendsInProgress = new();

        public DocumentService(
            IDocumentRepository documentRepository,
            ICustomerRepository customerRepository,
            IStaffRepository staffRepository,
            ICloudinaryService cloudinaryService,
            IPitchDeckFetcher fileFetcher,
            IDocumentMailer documentMailer)
        {
            _documentRepository = documentRepository;
            _customerRepository = customerRepository;
            _staffRepository = staffRepository;
            _cloudinaryService = cloudinaryService;
            _fileFetcher = fileFetcher;
            _documentMailer = documentMailer;
        }

        public async Task<List<DocumentDto>> GetAllAsync()
        {
            var documents = await _documentRepository.GetAllWithStaffAsync();
            return documents.Select(ToDto).ToList();
        }

        public async Task<DocumentDto> UploadAsync(
            string? title, string? description, Stream fileStream, string fileName, long sizeBytes, int staffId)
        {
            var staff = await RequireStaffAsync(staffId);
            var safeName = Path.GetFileName(fileName);

            var (url, publicId) = await _cloudinaryService.UploadRawAsync(
                fileStream,
                safeName,
                "delgender/documents");

            var now = DateTime.UtcNow;
            var document = new TeamDocument
            {
                Title = string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(safeName) : title.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                FileUrl = url,
                FilePublicId = publicId,
                FileName = safeName,
                SizeBytes = sizeBytes,
                UploadedByStaffId = staff.Id,
                CreatedAt = now,
                UpdatedAt = now
            };

            document = await _documentRepository.CreateAsync(document);
            document.UploadedByStaff = staff;
            return ToDto(document);
        }

        public async Task<(byte[] Bytes, string FileName)?> DownloadAsync(int id)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document is null)
            {
                return null;
            }

            var bytes = await _fileFetcher.FetchAsync(_cloudinaryService.GetRawDownloadUrl(document.FilePublicId));
            return (bytes, document.FileName);
        }

        public async Task<bool> DeleteAsync(int id, int staffId, bool isAdmin)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document is null)
            {
                return false;
            }

            if (!isAdmin && document.UploadedByStaffId != staffId)
            {
                throw new InvalidOperationException(
                    "Only the person who added this document, or an admin, can delete it.");
            }

            await _documentRepository.DeleteAsync(document);
            await _cloudinaryService.DeleteRawAsync(document.FilePublicId);
            return true;
        }

        public async Task<DocumentEmailTemplateDto?> GetEmailTemplateAsync(int clientId, int staffId)
        {
            var client = await _customerRepository.GetByIdAsync(clientId);
            if (client is null)
            {
                return null;
            }

            var staff = await RequireStaffAsync(staffId);

            var values = new Dictionary<string, string>
            {
                ["ContactName"] = FirstName(client.FullName),
                ["CompanyName"] = client.CompanyName,
                ["SenderName"] = $"{staff.Name} {staff.Surname}".Trim()
            };

            return new DocumentEmailTemplateDto
            {
                Subject = Fill(DocumentEmailContent.Subject, values),
                Body = Fill(DocumentEmailContent.Body, values).Replace("\r\n", "\n").Trim()
            };
        }

        /// <summary>
        /// Emails the ticked documents to one client:
        ///   1. lock the client so it can't be sent twice at once
        ///   2. check the client has an email address and every document still exists
        ///   3. download the documents
        ///   4. send the email with the subject and body the staff member settled on
        /// </summary>
        public async Task<DocumentsSentDto?> SendEmailAsync(EmailDocumentsDto dto, int staffId)
        {
            var client = await _customerRepository.GetByIdAsync(dto.ClientId);
            if (client is null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(client.Email))
            {
                throw new InvalidOperationException("This client doesn't have an email address.");
            }

            await RequireStaffAsync(staffId);

            var ids = dto.DocumentIds.Distinct().ToList();
            var documents = await _documentRepository.GetByIdsAsync(ids);

            if (documents.Count != ids.Count)
            {
                throw new KeyNotFoundException(
                    "One of the documents you ticked no longer exists. Refresh the page and try again.");
            }

            if (documents.Sum(d => d.SizeBytes) > MaxEmailBytes)
            {
                throw new ArgumentException(
                    "Those documents are too big to send in one email. Untick a few and send them in two emails.");
            }

            if (!SendsInProgress.TryAdd(client.Id, 0))
            {
                throw new InvalidOperationException("Documents are already being sent to this client. Give it a moment.");
            }

            try
            {
                var attachments = new List<DocumentAttachment>();
                foreach (var document in ids.Select(id => documents.First(d => d.Id == id)))
                {
                    var bytes = await _fileFetcher.FetchAsync(_cloudinaryService.GetRawDownloadUrl(document.FilePublicId));
                    attachments.Add(new DocumentAttachment(document.FileName, bytes));
                }

                await _documentMailer.SendAsync(new DocumentsEmail(
                    RecipientEmail: client.Email,
                    RecipientName: client.FullName,
                    Subject: dto.Subject,
                    Body: dto.Body,
                    Attachments: attachments));

                return new DocumentsSentDto
                {
                    RecipientEmail = client.Email,
                    DocumentCount = attachments.Count
                };
            }
            finally
            {
                SendsInProgress.TryRemove(client.Id, out _);
            }
        }

        private async Task<Staff> RequireStaffAsync(int staffId)
        {
            var staff = await _staffRepository.GetByIdAsync(staffId);
            if (staff is null)
            {
                throw new UnauthorizedAccessException("Your session is no longer valid.");
            }

            return staff;
        }

        private static string FirstName(string fullName)
        {
            var first = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return string.IsNullOrEmpty(first) ? "there" : first;
        }

        private static string Fill(string template, IReadOnlyDictionary<string, string> values)
        {
            var result = template;
            foreach (var pair in values)
            {
                result = result.Replace("{{" + pair.Key + "}}", pair.Value);
            }

            return result;
        }

        private static DocumentDto ToDto(TeamDocument document) => new()
        {
            Id = document.Id,
            Title = document.Title,
            Description = document.Description,
            FileName = document.FileName,
            SizeBytes = document.SizeBytes,
            UploadedByStaffId = document.UploadedByStaffId,
            UploadedByName = document.UploadedByStaff is null
                ? "A teammate"
                : $"{document.UploadedByStaff.Name} {document.UploadedByStaff.Surname}".Trim(),
            CreatedAt = document.CreatedAt
        };
    }
}
