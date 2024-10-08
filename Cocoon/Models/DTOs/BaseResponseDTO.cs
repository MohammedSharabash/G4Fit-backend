using G4Fit.Models.Enums;

namespace G4Fit.Models.DTOs
{
    public class BaseResponseDTO
    {
        private Errors Error = Errors.Success;
        public Errors ErrorCode
        {
            get
            {
                return Error;
            }
            set
            {
                Error = value;
                ErrorMessage = value.ToString();
            }
        }
        public string ErrorMessage { get; set; } = Errors.Success.ToString();
        public object Data { get; set; }
    }
}