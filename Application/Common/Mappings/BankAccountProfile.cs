using Application.BankAccounts.Dtos;
using AutoMapper;
using Domain.Models.Accounts;

namespace Application.Common.Mappings
{
    public class BankAccountProfile : Profile
    {
        public BankAccountProfile()
        {
            CreateMap<BankAccount, BankAccountDtoResponse>();
            CreateMap<CreateBankAccountDto, BankAccount>();
            // Later:
            // CreateMap<UpdateBankAccountDto, BankAccount>();
        }
    }
}
