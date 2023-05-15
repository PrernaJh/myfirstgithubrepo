namespace ParcelPrepGov.Web.Features.Bulletin
{

    public class Rule
    {
        public string MemberName
        {
            get;
            set;
        }

        public string Operator
        {
            get;
            set;
        }

        public string TargetValue
        {
            get;
            set;
        }

        public Rule(string MemberName, string Operator, string TargetValue)
        {
            this.MemberName = MemberName;
            this.Operator = Operator;
            this.TargetValue = TargetValue;
        }
        public Rule()
        {

        }
    }

}
