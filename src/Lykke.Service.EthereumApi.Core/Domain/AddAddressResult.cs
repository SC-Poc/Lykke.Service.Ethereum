namespace Lykke.Service.EthereumApi.Core.Domain
{
    public abstract class AddAddressResult
    {
        public static readonly HasAlreadyBeenAddedError HasAlreadyBeenAdded 
            = new HasAlreadyBeenAddedError();
        
        public static readonly SuccessResult Success 
            = new SuccessResult();
        
        
        public class SuccessResult : AddAddressResult
        {
            internal SuccessResult()
            {
                
            }
        }

        public class HasAlreadyBeenAddedError : AddAddressResult
        {
            internal HasAlreadyBeenAddedError()
            {
                
            }
        }
    }
}
