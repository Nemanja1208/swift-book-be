using System.ComponentModel.DataAnnotations;

namespace Application.BankAccounts.Dtos
{
    public class CreateBankAccountDto
    {
        [Required] // Use data annotation if you want to trigger the exception behavior for model binding in controller
        public string OwnerName { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
    }

    public class UpdateBankAccountDto
    {

        public string OwnerName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class BankAccountDtoResponse
    {
        public Guid Id { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime OpenedAt { get; set; }
    }
}
