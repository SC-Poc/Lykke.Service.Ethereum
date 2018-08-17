using FluentAssertions;
using Lykke.Service.EthereumCommon.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lykke.Service.EthereumCommon.Tests.Core
{
    [TestClass]
    public class AddressTests
    {
        [TestMethod]
        public void AddChecksum()
        {
            Address.AddChecksum("0x8fa24279718d868d5844527acdb001c6a1adbd08")
                .Should()
                .BeEquivalentTo("0x8FA24279718d868D5844527Acdb001c6a1AdbD08");
        }
        
        [TestMethod]
        public void ValidateFormatAndChecksum__ValidChecksum__TrueReturned()
        {
            Address.ValidateFormatAndChecksum("0x8FA24279718d868D5844527Acdb001c6a1AdbD08", false, false)
                .Should()
                .BeTrue();
        }
        
        [TestMethod]
        public void ValidateFormatAndChecksum__AllLowerCase_And_AllLowerCaseAllowed__TrueReturned()
        {
            Address.ValidateFormatAndChecksum("0x8fa24279718d868d5844527acdb001c6a1adbd08", true, false)
                .Should()
                .BeTrue();
        }
        
        [TestMethod]
        public void ValidateFormatAndChecksum__AllUpperCase_And_AllUpperCaseAllowed__TrueReturned()
        {
            Address.ValidateFormatAndChecksum("0x8FA24279718D868D5844527ACDB001C6A1ADBD08", false, true)
                .Should()
                .BeTrue();
        }
        
        [TestMethod]
        public void ValidateFormatAndChecksum__InvalidFormat__FalseReturned()
        {
            Address.ValidateFormatAndChecksum("8FA24279718d868D5844527Acdb001c6a1AdbD08", true, true)
                .Should()
                .BeFalse();
        }
        
        [TestMethod]
        public void ValidateFormatAndChecksum__InvalidChecksum__FalseReturned()
        {
            Address.ValidateFormatAndChecksum("0x8fa24279718d868d5844527acdb001c6a1adbd08", false, false)
                .Should()
                .BeFalse();
            
            Address.ValidateFormatAndChecksum("0x8FA24279718D868D5844527ACDB001C6A1ADBD08", false, false)
                .Should()
                .BeFalse();
            
            Address.ValidateFormatAndChecksum("0x8fa24279718D868D5844527ACDB001C6A1ADBD08", false, false)
                .Should()
                .BeFalse();
        }
    }
}
