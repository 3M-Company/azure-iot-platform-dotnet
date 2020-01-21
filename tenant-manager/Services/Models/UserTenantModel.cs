using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.TenantManager.Services.Models
{
    public class UserTenantModel : TableEntity
    {
        public UserTenantModel() { }

        public UserTenantModel(string userId, string tenantId)
        {
            this.PartitionKey = userId;
            this.RowKey = tenantId;
            this.Roles = string.Empty;
        }

        public UserTenantModel(string userId, string tenantId, string roles)
        {
            this.PartitionKey = userId;
            this.RowKey = tenantId;
            this.Roles = roles;
        }

        public string Roles { get; set; }

        // Define aliases for the partition and row keys
        public string UserId
        {
            get
            {
                return this.PartitionKey;
            }
        }

        public string TenantId
        {
            get
            {
                return this.RowKey;
            }
        }

        public List<string> RoleList
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<string>>(this.Roles);
                }
                catch
                {
                    return new List<string>(); // cant Deserialize return Empty List
                }
            }
        }
    }
}