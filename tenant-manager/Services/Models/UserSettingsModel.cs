using Microsoft.Azure.Cosmos.Table;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Models
{
    public class UserSettingsModel : TableEntity
    {
        public string Value { get; set; }

        public UserSettingsModel() { }

        public UserSettingsModel(string userId, string settingKey, string value)
        {
            this.PartitionKey = userId;
            this.RowKey = settingKey;
            this.Value = value;
        }

        // Define aliases for the partition and row keys
        public string UserId
        {
            get
            {
                return this.PartitionKey;
            }
        }

        public string SettingKey
        {
            get
            {
                return this.RowKey;
            }
        }
    }
}