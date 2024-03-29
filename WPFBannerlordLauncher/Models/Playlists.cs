using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFBannerlordLauncher.Models
{
    [SettingsSerializeAsAttribute(SettingsSerializeAs.Xml)]
    public class Playlists
    {
        public List<Playlist> playlists = new List<Playlist>();
    }
}
