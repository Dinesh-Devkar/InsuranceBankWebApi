﻿namespace InsuranceBankWebApiProject.DtoClasses.Common
{
    public class GetQueryDto
    {
        public string CustomerName { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Reply { get; set; }
        public string ContactDate { get; set; }
        public string CustomerId { get; set; }
        public int  QueryId { get; set; }
    }
}
