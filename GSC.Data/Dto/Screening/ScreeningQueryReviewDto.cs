namespace GSC.Data.Dto.Screening
{
    public class ScreeningQueryReviewDto : MyReviewDto
    {
        public string VariableName { get; set; }
        public string QueryStaus { get; set; }
        public string Reason { get; set; }
        public string Comments { get; set; }
        public short QueryLevel { get; set; }
    }
}