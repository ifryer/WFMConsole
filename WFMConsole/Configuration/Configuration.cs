using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace WFMConsole.Configuration
{
    public static class WFM
    {
        public static WFMConfig Config => (WFMConfig)ConfigurationManager.GetSection("WFM");
    }
    public class WFMConfig : ConfigurationSection
    {
        public bool IsAdministrator(string ldapId)
        {
            return Users.Cast<WFMUser>().Any(user => string.Equals(user.LdapId, ldapId, StringComparison.CurrentCultureIgnoreCase));
        }

        public WFMUser GetUser(string ldapId)
        {
            return (Users.Cast<WFMUser>().FirstOrDefault(u => string.Equals(u.LdapId, ldapId, StringComparison.CurrentCultureIgnoreCase)));
        }
        public List<WFMUser> GetUsers()
        {
            return (Users.Cast<WFMUser>()).ToList();
        }
        public class WFMUser : ConfigurationElement
        {
            [ConfigurationProperty("ldapId", IsRequired = true)]
            public string LdapId => (string)base["ldapId"];

            [ConfigurationProperty("isAdministrator", IsRequired = false, DefaultValue = false)]
            public bool IsAdministrator => bool.Parse(base["isAdministrator"].ToString());

            [ConfigurationProperty("isTemplateCreator", IsRequired = false, DefaultValue = false)]
            public bool IsTemplateCreator => bool.Parse(base["isTemplateCreator"].ToString());

            [ConfigurationProperty("canSeeAll", IsRequired = false, DefaultValue = false)]
            public bool CanSeeAll => bool.Parse(base["canSeeAll"].ToString());

            [ConfigurationProperty("title", IsRequired = false)]
            public string Title => (string)base["title"];

            [ConfigurationProperty("lastName", IsRequired = false)]
            public string LastName => (string)base["lastName"];
        }

        [ConfigurationProperty("users", IsRequired = true)]
        [ConfigurationCollection(typeof(WFMUsers))]
        public WFMUsers Users => (WFMUsers)base["users"];

        public class WFMUsers : ConfigurationElementCollection
        {
            protected override ConfigurationElement CreateNewElement()
            {
                return new WFMUser();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((WFMUser)element).LdapId;
            }
        }
    }
}