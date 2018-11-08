namespace Lykke.Service.EthereumApi.Core.Domain
{
    public abstract class RemoveAddressResult
    {
        public static readonly NotFoundError NotFound 
            = new NotFoundError();
        
        public static readonly SuccessResult Success 
            = new SuccessResult();
        
        
        public class SuccessResult : RemoveAddressResult
        {
            internal SuccessResult()
            {
                
            }
        }

        public class NotFoundError : RemoveAddressResult
        {
            internal NotFoundError()
            {
                
            }
        }
    }
}
