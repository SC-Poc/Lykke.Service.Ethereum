using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.EthereumSignApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SignApiSettings
    {
        [Optional]
        public DbSettings Db { get; set; }
    }
}
