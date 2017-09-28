using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WFMConsole.Models;

namespace WFMConsole.Classes
{
    public class Item
    {
        [Key]
        [MaxLength(100)]
        public string Key { get; set; }

        [MaxLength(500)]
        public string Value { get; set; }
    }

    //public class GoogleAuthContext : DbContext
    //{
    //    public DbSet<Item> Items { get; set; }
    //}



    public class EFDataStore : IDataStore
    {

        public EFDataStore(string name)
        {

        }

        public async Task ClearAsync()
        {
            using (var context = new OnyxEntities())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                await objectContext.ExecuteStoreCommandAsync("TRUNCATE TABLE [BUS_WFMDashboard_Google_Credentials]");
            }
        }

        public static async Task ClearAsyncStatic()
        {
            using (var context = new OnyxEntities())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                await objectContext.ExecuteStoreCommandAsync("TRUNCATE TABLE [BUS_WFMDashboard_Google_Credentials]");
            }
        }

        public async Task DeleteAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            using (var context = new OnyxEntities())
            {
                var generatedKey = GenerateStoredKey(key, typeof(T));
                var item = context.BUS_WFMDashboard_Google_Credentials.FirstOrDefault(x => x.Key == generatedKey);
                if (item != null)
                {
                    context.BUS_WFMDashboard_Google_Credentials.Remove(item);
                    await context.SaveChangesAsync();
                }
            }
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            using (var context = new OnyxEntities())
            {
                var generatedKey = GenerateStoredKey(key, typeof(T));
                var item = context.BUS_WFMDashboard_Google_Credentials.FirstOrDefault(x => x.Key == generatedKey);
                T value = item == null ? default(T) : JsonConvert.DeserializeObject<T>(item.Value);
                return Task.FromResult<T>(value);
            }
        }

        public async Task StoreAsync<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            using (var context = new OnyxEntities())
            {
                var generatedKey = GenerateStoredKey(key, typeof(T));
                string json = JsonConvert.SerializeObject(value);

                var item = await context.BUS_WFMDashboard_Google_Credentials.SingleOrDefaultAsync(x => x.Key == generatedKey);

                if (item == null)
                {
                    context.BUS_WFMDashboard_Google_Credentials.Add(new BUS_WFMDashboard_Google_Credentials { Key = generatedKey, Value = json });
                }
                else
                {
                    item.Value = json;
                }

                await context.SaveChangesAsync();
            }
        }

        private static string GenerateStoredKey(string key, Type t)
        {
            return string.Format("{0}-{1}", t.FullName, key);
        }
    }
}