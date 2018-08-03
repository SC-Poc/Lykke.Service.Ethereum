using FluentAssertions;
using Lykke.Service.EthereumSignApi.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lykke.Service.EthereumSignApi.Tests.Services
{
    [TestClass]
    public class SignServiceTests
    {
        [TestMethod]
        public void SignTransaction()
        {
            // TODO: Fix test
            
            const string transactionContext
                = "0xf8540184aabbccdd20f840df947567d83b7b8d80addcb281a71d54fc7b3364ffed82271086000000606060df947567d83b7b8d80addcb281a71d54fc7b3364ffed824e208600000060606081808252088083bc614ec0";

            const string privateKey
                = "0x7582be841ca040aa940fff6c05773129e135623e41acce3e0b8ba520dc1ae26a";

            const string expectedSignedtransaction
                = "0xf8970184aabbccdd20f840df947567d83b7b8d80addcb281a71d54fc7b3364ffed82271086000000606060df947567d83b7b8d80addcb281a71d54fc7b3364ffed824e208600000060606081808252088083bc614ec0b841f76f3c91a834165872aa9464fc55b03a13f46ea8d3b858e528fcceaf371ad6884193c3f313ff8effbb57fe4d1adc13dceb933bedbf9dbb528d2936203d5511df00";
            
            var signService = new SignService();

            signService.SignTransaction(transactionContext, privateKey)
                .Should().BeEquivalentTo(expectedSignedtransaction);
        }
    }
}
