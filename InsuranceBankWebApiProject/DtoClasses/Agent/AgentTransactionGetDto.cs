namespace InsuranceBankWebApiProject.DtoClasses.Agent
{
    public class AgentTransactionGetDto
    {
        public double Amount { get; set; }
        public string AgentId { get; set; }

        public string TransactionDate { get; set; }
        public string TransactionType { get; set; }
    }
}
