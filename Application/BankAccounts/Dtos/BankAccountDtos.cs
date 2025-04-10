namespace Application.BankAccounts.Dtos
{
    public class CreateBankAccountDto
    {
        public string OwnerName { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public decimal? InitialBalance { get; set; } // Optional — map to Balance
        public bool IsActive { get; set; } = true; // Optional toggle
    }


    public class UpdateBankAccountDto
    {
        public string OwnerName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal Balance { get; set; }
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
