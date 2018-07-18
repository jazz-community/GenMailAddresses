using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RuS.Jazz
{
    public static class JazzConstants
    {
        public const string jazzProc        = "jp";
        public const string jazzProcNsUrl   = "http://jazz.net/xmlns/prod/jazz/process/1.0/";
        public const string jazzProcNsUrl06 = "http://jazz.net/xmlns/prod/jazz/process/0.6/";
        public const string jazzProcNum     = "jp06";
    }

    public sealed class JazzAccessRTCServer
    {
        static readonly AccessRTCServer _instance = new AccessRTCServer();

        public static void SetAccessParams(string serverURL, string userName, string passWord)
        {
            _instance.RTCServerURL = serverURL;
            _instance.User = userName;
            _instance.Password = passWord;
        }

        public static AccessRTCServer GetJazzAccessRTCServer()
        {
            return _instance;
        }
    }

    public class JazzRole
    {
        private string _id;
        public string Id
        {
            get { return _id; }
        }
        private string _label;
        public string Label
        {
            get { return _label; }
        }
        private string _url;
        public string URL
        {
            get { return _url; }
        }
        private string _description;
        public string Description
        {
            get { return _description; }
        }

        public JazzRole(string url)
        {
            _url = url;
            AccessRTCServer accServ = JazzAccessRTCServer.GetJazzAccessRTCServer();

            XmlDocument xml = accServ.getXml(_url);
            XmlNamespaceManager nsMgr = accServ.buildNamespaceManager(xml, new string[] { JazzConstants.jazzProcNum });

            string currXPath = "//"+ JazzConstants.jazzProcNum + ":id";
            XmlNode node = xml.SelectSingleNode(currXPath, nsMgr);
            _id = node.InnerText;

            currXPath = "//" + JazzConstants.jazzProcNum + ":label";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            _label = node.InnerText;

            currXPath = "//" + JazzConstants.jazzProcNum + ":description";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            _description = node.InnerText;
        }
    }

    public sealed class JazzRoleCache
    {
        static readonly JazzRoleCache _instance = new JazzRoleCache();
        //private static List<JazzRole> _role_list;
        private static Dictionary<string, JazzRole> _role_dict;

        private JazzRoleCache()
        {
            //_role_list = new List<JazzRole>();
            _role_dict = new Dictionary<string, JazzRole>();
        }

        public static JazzRoleCache GetJazzRoleCache()
        {
            return _instance;
        }

        public JazzRole GetRole(string url)
        {
            //foreach (JazzRole r in _role_list)
            //{
            //    if (r.URL.Equals(url))
            //        return r;
            //}
            //return null;
            JazzRole ret = null;
            if (_role_dict.TryGetValue(url, out ret))
                return ret;
            else
                return null;
        }

        public void AddRole(JazzRole role)
        {
            //_role_list.Add(role);
            _role_dict.Add(role.URL, role);
        }

        public JazzRole GetOrAddRole(string url)
        {
            JazzRole r = GetRole(url);
            if (r == null)
            {
                r = new JazzRole(url);
                AddRole(r);
            }
            return r;
        }
    }

    public class JazzUser
    {
        private string _name;
        public string Name
        {
            get { return _name; }
        }
        private string _nick;
        public string Nick
        {
            get { return _nick; }
        }
        private string _mail;
        public string MailAddress
        {
            get { return _mail; }
        }
        private string _url;
        public string URL
        {
            get { return _url; }
        }
        private bool _archived = false;
        public bool isArchived
        {
            get { return _archived; }
        }

        public JazzUser(string name, string url)
        {
            _name = name;
            _url = url;
        }

        public JazzUser(string url)
        {
            _url = url;
            AccessRTCServer accServ = JazzAccessRTCServer.GetJazzAccessRTCServer();

            XmlDocument xml = accServ.getXml(_url, true);
            //XmlNamespaceManager nsMgr = accServ.buildNamespaceManager(xml, new string[] { "rdf", "j.0", "j.1" });
            XmlNamespaceManager nsMgr = accServ.buildNamespaceManager(xml, new string[] { "rdf", "foaf", "jfs" });

            //string currXPath = "//j.0:name";
            string currXPath = "//foaf:name";
            XmlNode node = xml.SelectSingleNode(currXPath, nsMgr);
            _name = node.InnerText;

            //currXPath = "//j.0:nick";
            currXPath = "//foaf:nick";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            _nick = node.InnerText;

            //currXPath = "//j.0:mbox/@rdf:resource";
            currXPath = "//foaf:mbox/@rdf:resource";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            _mail = node.InnerText.Replace("mailto:", string.Empty);

            //currXPath = "//j.1:archived";
            currXPath = "//jfs:archived";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            if (node.InnerText == "true")
                _archived = true;
        }
    }

    public sealed class JazzUserRepository
    {
        static readonly JazzUserRepository _instance = new JazzUserRepository();
        //private static List<JazzUser> _user_list;
        private static Dictionary<string, JazzUser> _user_dict;

        private JazzUserRepository()
        {
            //_user_list = new List<JazzUser>();
            _user_dict = new Dictionary<string, JazzUser>();
        }

        public static JazzUserRepository GetJazzUserRepository()
        {
            return _instance;
        }

        public JazzUser GetUser(string url)
        {
            //foreach (JazzUser u in _user_list)
            //{
            //    if (u.URL.Equals(url))
            //        return u;
            //}
            //return null;
            JazzUser ret = null;
            if (_user_dict.TryGetValue(url, out ret))
                return ret;
            else
                return null;
        }

        public JazzUser GetUserByName(string userName)
        {
            foreach (JazzUser ju in _user_dict.Values)
                if (ju.Name == userName)
                    return ju;
            return null;
        }

        public void AddUser(JazzUser user)
        {
            //_user_list.Add(user);
            _user_dict.Add(user.URL, user);
        }

        public JazzUser GetOrAddUser(string url, AccessRTCServer accServ)
        {
            JazzUser u = GetUser(url);
            if (u == null)
            {
                u = new JazzUser(url);
                AddUser(u);
            }
            return u;
        }
    }

    public class JazzMember
    {
        private string _url;
        public string URL
        {
            get { return _url; }
        }
        private JazzUser _user;
        public JazzUser JazzMemberUser
        {
            get { return _user; }
        }

        public bool PostIsPending {get; set;}

        private List<JazzRole> _roles;
        private AccessRTCServer _accServ = null;

        public JazzMember(string url, AccessRTCServer accServ)
        {
            _url = url;
            _accServ = accServ;
            PostIsPending = false;

            XmlDocument xml = accServ.getXml(url);
            XmlNamespaceManager nsMgr = accServ.buildNamespaceManager(xml, new string[] { JazzConstants.jazzProcNum });
            //nsMgr.AddNamespace(JazzConstants.jazzProc, JazzConstants.jazzProcNsUrl);

            string currXPath = "/" + JazzConstants.jazzProcNum + ":member/" + JazzConstants.jazzProcNum + ":user-url";
            XmlNode node = xml.SelectSingleNode(currXPath, nsMgr);
            //_user = new JazzUser(node.InnerText, accServ);
            _user = JazzUserRepository.GetJazzUserRepository().GetOrAddUser(node.InnerText, accServ);

            currXPath = "/" + JazzConstants.jazzProcNum + ":member/" + JazzConstants.jazzProcNum + ":role-assignments/" + JazzConstants.jazzProcNum + ":role-assignment";
            XmlNodeList nodeList = xml.SelectNodes(currXPath, nsMgr);
            if (nodeList.Count > 0)
            {
                _roles = new List<JazzRole>();

                foreach (XmlNode n in nodeList)
                {
                    currXPath = "./" + JazzConstants.jazzProcNum + ":role-url";
                    node = n.SelectSingleNode(currXPath, nsMgr);
                    //_roles.Add(new JazzRole(node.InnerText, accServ));
                    _roles.Add(JazzRoleCache.GetJazzRoleCache().GetOrAddRole(node.InnerText));
                }
            }
        }

        public JazzMember(string userName)
        {
            _accServ = JazzAccessRTCServer.GetJazzAccessRTCServer();
            _user = JazzUserRepository.GetJazzUserRepository().GetUserByName(userName);
        }

        public void SetMemberURL(string memberURL)
        {
            _url = memberURL;
        }

        public System.Collections.IEnumerable JazzMemberRoles()
        {
            if (_roles != null)
                for (int i = 0; i < _roles.Count; i++)
                    yield return _roles[i];
        }

        public bool JazzMemberHasRoleWithLabel(string role)
        {
            if (_roles != null)
                for (int i = 0; i < _roles.Count; i++)
                    if (_roles[i].Label.Equals(role, StringComparison.OrdinalIgnoreCase))
                        return true;
            return false;
        }

        public bool JazzMemberHasDefaultRoleOnly()
        {
            if (_roles == null)
                return true;
            else if(_roles.Count < 2)
                return true;
            else
                return false;
        }

        public void AssignRole(JazzRole jazzRole)
        {
            XmlDocument px = new XmlDocument();

            px.InsertBefore(px.CreateXmlDeclaration("1.0", "UTF-8", null), px.DocumentElement);
            XmlElement rootNode = px.CreateElement(JazzConstants.jazzProcNum + ":role-assignments", JazzConstants.jazzProcNsUrl06);
            px.AppendChild(rootNode);

            XmlElement roleAs = px.CreateElement(JazzConstants.jazzProcNum + ":role-assignment", JazzConstants.jazzProcNsUrl06);
            px.DocumentElement.AppendChild(roleAs);

            XmlText roleUrlCont = px.CreateTextNode(jazzRole.URL);
            XmlElement roleUrl = px.CreateElement(JazzConstants.jazzProcNum + ":role-url", JazzConstants.jazzProcNsUrl06);
            roleAs.AppendChild(roleUrl);
            roleUrl.AppendChild(roleUrlCont);

            _accServ.postXml(_url + "/role-assignments", px);

            _roles.Add(jazzRole);
        }

        public void UnassignRole(string role)
        {
            if (_roles != null)
                for (int i = 0; i < _roles.Count; i++)
                    if (_roles[i].Label.Equals(role, StringComparison.OrdinalIgnoreCase))
                    {
                        string roleAssignmentURL = _url + "/role-assignments/" + _roles[i].URL.Substring(_roles[i].URL.LastIndexOf('/')+1);
                        if (_accServ.delete(roleAssignmentURL) == System.Net.HttpStatusCode.OK)
                            _roles.RemoveAt(i);
                        return;
                    }            
        }

        public void AddRole(JazzRole jazzRole)
        {
            if (_roles == null)
                _roles = new List<JazzRole>();
            if (!JazzMemberHasRoleWithLabel(jazzRole.Label))
                _roles.Add(jazzRole);
        }

        public void SetRoles()
        {
            if ((_roles != null) && (_roles.Count > 0))
            {
                XmlDocument px = new XmlDocument();

                px.InsertBefore(px.CreateXmlDeclaration("1.0", "UTF-8", null), px.DocumentElement);
                XmlElement rootNode = px.CreateElement(JazzConstants.jazzProcNum + ":role-assignments", JazzConstants.jazzProcNsUrl06);
                px.AppendChild(rootNode);

                foreach (JazzRole jr in _roles)
                {
                    XmlElement roleAs = px.CreateElement(JazzConstants.jazzProcNum + ":role-assignment", JazzConstants.jazzProcNsUrl06);
                    px.DocumentElement.AppendChild(roleAs);

                    XmlText roleUrlCont = px.CreateTextNode(jr.URL);
                    XmlElement roleUrl = px.CreateElement(JazzConstants.jazzProcNum + ":role-url", JazzConstants.jazzProcNsUrl06);
                    roleAs.AppendChild(roleUrl);
                    roleUrl.AppendChild(roleUrlCont);
                }
                _accServ.putXml(_url + "/role-assignments", px);
            }
            else
                _accServ.delete(_url + "/role-assignments");
        }
    }

    public class JazzTeamArea
    {
        private string _name;
        public string Name
        {
            get { return _name; }
        }
        private string _url;
        public string URL
        {
            get { return _url; }
        }
        private string _summary;
        public string Summary
        {
            get { return _summary; }
        }
        private string _description;
        public string Description
        {
            get { return _description; }
        }
        private string _admins_url;
        private string _members_url;
        private string _roles_url;
        private bool _archived = false;
        public bool isArchived
        {
            get { return _archived; }
        }

        private List<JazzTeamArea> _ta_list = null;
        private List<JazzUser> _adm_list = null;
        private List<JazzMember> _mem_list = null;
        private List<JazzRole> _role_list = null;
        private AccessRTCServer _accServ = null;

        public JazzTeamArea(string name, string url)
        {
            _name = name;
            _url = url;
        }

        public JazzTeamArea(string name, string url, AccessRTCServer accServ)
        {
            _name = name;
            _url = url;
            _accServ = accServ;

            XmlDocument xml = accServ.getXml(_url + "?includeArchived=true");
            XmlNamespaceManager nsMgr = accServ.buildNamespaceManager(xml, new string[] { JazzConstants.jazzProcNum });
            nsMgr.AddNamespace(JazzConstants.jazzProc, JazzConstants.jazzProcNsUrl);

            string currXPath = "/" + JazzConstants.jazzProcNum + ":team-area/" + JazzConstants.jazzProcNum + ":summary";
            XmlNode node = xml.SelectSingleNode(currXPath, nsMgr);
            _summary = node.InnerText;

            currXPath = "/" + JazzConstants.jazzProcNum + ":team-area/" + JazzConstants.jazzProcNum + ":description";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            _description = node.InnerText;

            currXPath = "/" + JazzConstants.jazzProcNum + ":team-area/" + JazzConstants.jazzProcNum + ":children/" + JazzConstants.jazzProcNum + ":team-area";
            XmlNodeList nodeList = xml.SelectNodes(currXPath, nsMgr);
            if (nodeList.Count > 0)
            {
                _ta_list = new List<JazzTeamArea>();

                foreach (XmlNode n in nodeList)
                {
                    currXPath = "./@" + JazzConstants.jazzProcNum + ":name";
                    XmlNode currNode = n.SelectSingleNode(currXPath, nsMgr);
                    string ta_name = currNode.InnerText;
                    currXPath = "./" + JazzConstants.jazzProcNum + ":url";
                    currNode = n.SelectSingleNode(currXPath, nsMgr);
                    _ta_list.Add(new JazzTeamArea(ta_name, currNode.InnerText, _accServ));
                }
            }

            currXPath = "/" + JazzConstants.jazzProcNum + ":team-area/" + JazzConstants.jazzProc + ":admins-url";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            _admins_url = node.InnerText;

            currXPath = "/" + JazzConstants.jazzProcNum + ":team-area/" + JazzConstants.jazzProcNum + ":members-url";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            _members_url = node.InnerText;

            currXPath = "/" + JazzConstants.jazzProcNum + ":team-area/" + JazzConstants.jazzProcNum + ":roles-url";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            _roles_url = node.InnerText;

            currXPath = "/" + JazzConstants.jazzProcNum + ":team-area/" + JazzConstants.jazzProcNum + ":archived";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            if (node.InnerText == "true")
                _archived = true;
        }

        public JazzTeamArea(XmlNode taNode, AccessRTCServer accServ)
        {
            _accServ = accServ;

            XmlNamespaceManager nsMgr = accServ.buildNamespaceManager(taNode, new string[] { JazzConstants.jazzProcNum });
            nsMgr.AddNamespace(JazzConstants.jazzProc, JazzConstants.jazzProcNsUrl);

            string currXPath = "./@" + JazzConstants.jazzProcNum + ":name";
            XmlNode node = taNode.SelectSingleNode(currXPath, nsMgr);
            _name = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProcNum + ":url";
            node = taNode.SelectSingleNode(currXPath, nsMgr);
            _url = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProcNum + ":summary";
            node = taNode.SelectSingleNode(currXPath, nsMgr);
            _summary = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProcNum + ":description";
            node = taNode.SelectSingleNode(currXPath, nsMgr);
            _description = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProcNum + ":children/" + JazzConstants.jazzProcNum + ":team-area";
            XmlNodeList nodeList = taNode.SelectNodes(currXPath, nsMgr);
            if (nodeList.Count > 0)
            {
                _ta_list = new List<JazzTeamArea>();

                foreach (XmlNode n in nodeList)
                {
                    currXPath = "./@" + JazzConstants.jazzProcNum + ":name";
                    XmlNode currNode = n.SelectSingleNode(currXPath, nsMgr);
                    string ta_name = currNode.InnerText;
                    currXPath = "./" + JazzConstants.jazzProcNum + ":url";
                    currNode = n.SelectSingleNode(currXPath, nsMgr);
                    _ta_list.Add(new JazzTeamArea(ta_name, currNode.InnerText, _accServ));
                    //_ta_list.Add(new JazzTeamArea(n, _accServ));
                }
            }

            currXPath = "./" + JazzConstants.jazzProc + ":admins-url";
            node = taNode.SelectSingleNode(currXPath, nsMgr);
            _admins_url = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProcNum + ":members-url";
            node = taNode.SelectSingleNode(currXPath, nsMgr);
            _members_url = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProcNum + ":roles-url";
            node = taNode.SelectSingleNode(currXPath, nsMgr);
            _roles_url = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProcNum + ":archived";
            node = taNode.SelectSingleNode(currXPath, nsMgr);
            if (node.InnerText == "true")
                _archived = true;
        }

        public System.Collections.IEnumerable JazzTeamAreas()
        {
            if (_ta_list != null)
                for (int i = 0; i < _ta_list.Count; i++)
                    yield return _ta_list[i];
        }

        public JazzTeamArea GetJazzSubTeamAreaByName(string name)
        {
            if (_ta_list != null)
                for (int i = 0; i < _ta_list.Count; i++)
                    if (_ta_list[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        return _ta_list[i];
            return null;
        }

        public System.Collections.IEnumerable JazzTeamAdmins()
        {
            if (_adm_list == null)
            {
                _adm_list = new List<JazzUser>();

                XmlDocument xml = _accServ.getXml(_admins_url);
                XmlNamespaceManager nsMgr = _accServ.buildNamespaceManager(xml, new string[] { JazzConstants.jazzProcNum });
                nsMgr.AddNamespace(JazzConstants.jazzProc, JazzConstants.jazzProcNsUrl);

                string teamAreaXPath = "//" + JazzConstants.jazzProc + ":admin";
                XmlNodeList nodeList = xml.SelectNodes(teamAreaXPath, nsMgr);
                foreach (XmlNode n in nodeList)
                {
                    string currXPath = "./" + JazzConstants.jazzProc + ":user-url";
                    XmlNode currNode = n.SelectSingleNode(currXPath, nsMgr);
                    //_adm_list.Add(new JazzUser(currNode.InnerText, _accServ));
                    _adm_list.Add(JazzUserRepository.GetJazzUserRepository().GetOrAddUser(currNode.InnerText, _accServ));
                }
            }
            if (_adm_list != null)
                for (int i = 0; i < _adm_list.Count; i++)
                    yield return _adm_list[i];
        }

        private void GetMembers()
        {
            _mem_list = new List<JazzMember>();

            XmlDocument xml = _accServ.getXml(_members_url);
            XmlNamespaceManager nsMgr = _accServ.buildNamespaceManager(xml, new string[] { JazzConstants.jazzProcNum });
            //nsMgr.AddNamespace(JazzConstants.jazzProc, JazzConstants.jazzProcNsUrl);

            string teamAreaXPath = "//" + JazzConstants.jazzProcNum + ":member";
            XmlNodeList nodeList = xml.SelectNodes(teamAreaXPath, nsMgr);
            foreach (XmlNode n in nodeList)
            {
                string currXPath = "./" + JazzConstants.jazzProcNum + ":url";
                XmlNode currNode = n.SelectSingleNode(currXPath, nsMgr);
                _mem_list.Add(new JazzMember(currNode.InnerText, _accServ));
            }
        }

        public System.Collections.IEnumerable JazzTeamMembers()
        {
            if (_mem_list == null)
            {
                GetMembers();
            }
            if (_mem_list != null)
                for (int i = 0; i < _mem_list.Count; i++)
                    yield return _mem_list[i];
        }

        public bool HasMemberWithName(string MemberName)
        {
            if (_mem_list == null)
                GetMembers();

            if (_mem_list != null)
                for (int i = 0; i < _mem_list.Count; i++)
                    if (_mem_list[i].JazzMemberUser.Name.Equals(MemberName, StringComparison.OrdinalIgnoreCase))
                        return true;
            return false;
        }

        public JazzMember GetMemberWithName(string MemberName)
        {
            if (_mem_list == null)
                GetMembers();

            if (_mem_list != null)
                for (int i = 0; i < _mem_list.Count; i++)
                    if (_mem_list[i].JazzMemberUser.Name.Equals(MemberName, StringComparison.OrdinalIgnoreCase))
                        return _mem_list[i];
            return null;
        }

        public void DeleteMemberWithName(string MemberName)
        {
            if (_mem_list == null)
                GetMembers();

            if (_mem_list != null)
                for (int i = 0; i < _mem_list.Count; i++)
                    if (_mem_list[i].JazzMemberUser.Name.Equals(MemberName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (_accServ.delete(_mem_list[i].URL) == System.Net.HttpStatusCode.OK)
                            _mem_list.RemoveAt(i);
                        return;
                    }
        }

        public void AddMemberPending(JazzMember member)
        {
            member.PostIsPending = true;
            _mem_list.Add(member);
        }

        public void PostNewMembers()
        {
            XmlDocument px = new XmlDocument();

            px.InsertBefore(px.CreateXmlDeclaration("1.0", "UTF-8", null), px.DocumentElement);
            XmlElement rootNode  = px.CreateElement(JazzConstants.jazzProcNum + ":members", JazzConstants.jazzProcNsUrl06);
            px.AppendChild(rootNode);
            int memPostCount = 0;
            foreach (JazzMember jm in JazzTeamMembers())
            {
                if (jm.PostIsPending)
                {
                    memPostCount++;
                    XmlElement mem = px.CreateElement(JazzConstants.jazzProcNum + ":member", JazzConstants.jazzProcNsUrl06);
                    px.DocumentElement.AppendChild(mem);
                    //XmlText usrCont = px.CreateTextNode(_url + "/members/" + jm.JazzMemberUser.Name);
                    //XmlElement usr = px.CreateElement(JazzConstants.jazzProcNum + ":usr", JazzConstants.jazzProcNsUrl);
                    //mem.AppendChild(usr);
                    //usr.AppendChild(usrCont);
                    XmlText usrUrlCont = px.CreateTextNode(jm.JazzMemberUser.URL);
                    XmlElement usrUrl = px.CreateElement(JazzConstants.jazzProcNum + ":user-url", JazzConstants.jazzProcNsUrl06);
                    mem.AppendChild(usrUrl);
                    usrUrl.AppendChild(usrUrlCont);
                    int memRoleCount = 0;
                    XmlElement roleAss = null;
                    foreach (JazzRole jr in jm.JazzMemberRoles())
                    {
                        if (memRoleCount == 0)
                        {
                            roleAss = px.CreateElement(JazzConstants.jazzProcNum + ":role-assignments", JazzConstants.jazzProcNsUrl06);
                            mem.AppendChild(roleAss);
                        }
                        XmlElement roleAs = px.CreateElement(JazzConstants.jazzProcNum + ":role-assignment", JazzConstants.jazzProcNsUrl06);
                        XmlText roleUrlCont = px.CreateTextNode(jr.URL);
                        XmlElement roleUrl = px.CreateElement(JazzConstants.jazzProcNum + ":role-url", JazzConstants.jazzProcNsUrl06);
                        roleAss.AppendChild(roleAs);
                        roleAs.AppendChild(roleUrl);
                        roleUrl.AppendChild(roleUrlCont);
                        memRoleCount++;
                    }
                    jm.PostIsPending = false;
                }
            }
            if (memPostCount > 0)
                _accServ.postXml(_url + "/members", px);
        }

        public void AddMember(JazzMember member)
        {
            XmlDocument px = new XmlDocument();

            px.InsertBefore(px.CreateXmlDeclaration("1.0", "UTF-8", null), px.DocumentElement);
            XmlElement rootNode = px.CreateElement(JazzConstants.jazzProcNum + ":members", JazzConstants.jazzProcNsUrl06);
            px.AppendChild(rootNode);
            XmlElement mem = px.CreateElement(JazzConstants.jazzProcNum + ":member", JazzConstants.jazzProcNsUrl06);
            px.DocumentElement.AppendChild(mem);
            XmlText usrUrlCont = px.CreateTextNode(member.JazzMemberUser.URL);
            XmlElement usrUrl = px.CreateElement(JazzConstants.jazzProcNum + ":user-url", JazzConstants.jazzProcNsUrl06);
            mem.AppendChild(usrUrl);
            usrUrl.AppendChild(usrUrlCont);

            string mem_url;
            _accServ.postXml(_url + "/members", px, out mem_url);
            member.SetMemberURL(mem_url);

            _mem_list.Add(member);
        }

        public System.Collections.IEnumerable JazzTeamRoles()
        {
            if (_role_list == null)
            {
                _role_list = new List<JazzRole>();

                XmlDocument xml = _accServ.getXml(_roles_url);
                XmlNamespaceManager nsMgr = _accServ.buildNamespaceManager(xml, new string[] { JazzConstants.jazzProcNum });
                //nsMgr.AddNamespace(JazzConstants.jazzProc, JazzConstants.jazzProcNsUrl);

                string teamAreaXPath = "//" + JazzConstants.jazzProcNum + ":role";
                XmlNodeList nodeList = xml.SelectNodes(teamAreaXPath, nsMgr);
                foreach (XmlNode n in nodeList)
                {
                    string currXPath = "./" + JazzConstants.jazzProcNum + ":url";
                    XmlNode currNode = n.SelectSingleNode(currXPath, nsMgr);
                    //_role_list.Add(new JazzRole(currNode.InnerText, _accServ));
                    _role_list.Add(JazzRoleCache.GetJazzRoleCache().GetOrAddRole(currNode.InnerText));
                }
            }
            if (_role_list != null)
                for (int i = 0; i < _role_list.Count; i++)
                    yield return _role_list[i];
        }

        public JazzRole GetJazzRoleByLabel(string role)
        {
            foreach (JazzRole jr in this.JazzTeamRoles())
                if (jr.Label == role)
                    return jr;
            return null;
        }
    }

    public class JazzProjectArea
    {
        private string _name;
        public string Name
        {
            get { return _name; }
        }
        private string _url;
        public string URL
        {
            get { return _url; }
        }
        private string _summary;
        public string Summary
        {
            get { return _summary; }
        }
        private string _description;
        public string Description
        {
            get { return _description; }
        }
        private string _team_areas_url;
        //public string TeamAreaListURL
        //{
        //    get { return _team_areas_url; }
        //}
        private string _admins_url;
        private string _members_url;
        private string _roles_url;
        private bool _archived = true;
        public bool isArchived
        {
            get { return _archived; }
        }

        private List<JazzTeamArea> _ta_list = null;
        private List<JazzUser> _adm_list = null;
        private List<JazzMember> _mem_list = null;
        private List<JazzRole> _role_list = null;
        private AccessRTCServer _accServ = null;

        public JazzProjectArea(string name, string url)
        {
            _name = name;
            _url = url;
        }

        public JazzProjectArea(string name, string url, AccessRTCServer accServ)
        {
            _name = name;
            _url = url;
            _accServ = accServ;

            XmlDocument xml = accServ.getXml(_url + "?includeArchived=true");
            XmlNamespaceManager nsMgr = accServ.buildNamespaceManager(xml, new string[] { JazzConstants.jazzProcNum });
            nsMgr.AddNamespace(JazzConstants.jazzProc, JazzConstants.jazzProcNsUrl);

            string currXPath = "/" + JazzConstants.jazzProcNum + ":project-area/" + JazzConstants.jazzProcNum + ":summary";
            XmlNode node = xml.SelectSingleNode(currXPath, nsMgr);
            _summary = node.InnerText;

            currXPath = "/" + JazzConstants.jazzProcNum + ":project-area/" + JazzConstants.jazzProcNum + ":description";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            _description = node.InnerText;

            currXPath = "/" + JazzConstants.jazzProcNum + ":project-area/" + JazzConstants.jazzProcNum + ":team-areas-url";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            _team_areas_url = node.InnerText;

            currXPath = "/" + JazzConstants.jazzProcNum + ":project-area/" + JazzConstants.jazzProc + ":admins-url";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            _admins_url = node.InnerText;

            currXPath = "/" + JazzConstants.jazzProcNum + ":project-area/" + JazzConstants.jazzProcNum + ":members-url";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            _members_url = node.InnerText;

            currXPath = "/" + JazzConstants.jazzProcNum + ":project-area/" + JazzConstants.jazzProcNum + ":roles-url";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            _roles_url = node.InnerText;

            currXPath = "/" + JazzConstants.jazzProcNum + ":project-area/" + JazzConstants.jazzProcNum + ":archived";
            node = xml.SelectSingleNode(currXPath, nsMgr);
            if (node.InnerText == "false")
                _archived = false;
        }

        public JazzProjectArea(XmlNode paNode, AccessRTCServer accServ)
        {
            _accServ = accServ;

            XmlNamespaceManager nsMgr = accServ.buildNamespaceManager(paNode, new string[] { JazzConstants.jazzProcNum });
            nsMgr.AddNamespace(JazzConstants.jazzProc, JazzConstants.jazzProcNsUrl);

            string currXPath = "./@" + JazzConstants.jazzProcNum + ":name";
            XmlNode node = paNode.SelectSingleNode(currXPath, nsMgr);
            _name = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProcNum + ":url";
            node = paNode.SelectSingleNode(currXPath, nsMgr);
            _url = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProcNum + ":summary";
            node = paNode.SelectSingleNode(currXPath, nsMgr);
            _summary = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProcNum + ":description";
            node = paNode.SelectSingleNode(currXPath, nsMgr);
            _description = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProcNum + ":team-areas-url";
            node = paNode.SelectSingleNode(currXPath, nsMgr);
            _team_areas_url = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProc + ":admins-url";
            node = paNode.SelectSingleNode(currXPath, nsMgr);
            _admins_url = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProcNum + ":members-url";
            node = paNode.SelectSingleNode(currXPath, nsMgr);
            _members_url = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProcNum + ":roles-url";
            node = paNode.SelectSingleNode(currXPath, nsMgr);
            _roles_url = node.InnerText;

            currXPath = "./" + JazzConstants.jazzProcNum + ":archived";
            node = paNode.SelectSingleNode(currXPath, nsMgr);
            if (node.InnerText == "false")
                _archived = false;
        }

        public System.Collections.IEnumerable JazzTeamAreas()
        {
            if (_ta_list == null)
            {
                _ta_list = new List<JazzTeamArea>();

                XmlDocument xml = _accServ.getXml(_team_areas_url + "?includeArchived=true");
                XmlNamespaceManager nsMgr = _accServ.buildNamespaceManager(xml, new string[] { JazzConstants.jazzProcNum });

                string teamAreaXPath = "/" + JazzConstants.jazzProcNum + ":team-areas/" + JazzConstants.jazzProcNum + ":team-area";
                XmlNodeList nodeList = xml.SelectNodes(teamAreaXPath, nsMgr);
                foreach (XmlNode n in nodeList)
                {
                    //string currXPath = "./@" + JazzConstants.jazzProcNum + ":name";
                    //XmlNode currNode = n.SelectSingleNode(currXPath, nsMgr);
                    //string ta_name = currNode.InnerText;
                    //currXPath = "./" + JazzConstants.jazzProcNum + ":url";
                    //currNode = n.SelectSingleNode(currXPath, nsMgr);
                    //_ta_list.Add(new JazzTeamArea(ta_name, currNode.InnerText, _accServ));
                    _ta_list.Add(new JazzTeamArea(n, _accServ));
                }
            }
            if (_ta_list != null)
                for (int i = 0; i < _ta_list.Count; i++)
                    yield return _ta_list[i];
        }

        public JazzTeamArea GetJazzTeamAreaByName(string name)
        {
            if (_ta_list == null)
            {
                XmlDocument xml = _accServ.getXml(_team_areas_url + "?includeArchived=true");
                //nsMgr = _accServ.buildNamespaceManager(xml, new string[] { "rdf", "oslc", "dcterms" });
                XmlNamespaceManager nsMgr = _accServ.buildNamespaceManager(xml, new string[] { JazzConstants.jazzProcNum });

                //currXPath = "//oslc:ServiceProvider";
                string currXPath = "/" + JazzConstants.jazzProcNum + ":team-areas/" + JazzConstants.jazzProcNum + ":team-area[@" + JazzConstants.jazzProcNum + ":name='" + name + "']";
                XmlNode n = xml.SelectSingleNode(currXPath, nsMgr);
                if (n != null)
                    return new JazzTeamArea(n, _accServ);
            }
            else
                for (int i = 0; i < _ta_list.Count; i++)
                    if (_ta_list[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        return _ta_list[i];
            return null;
        }

        public System.Collections.IEnumerable JazzProjectAdmins()
        {
            if (_adm_list == null)
            {
                _adm_list = new List<JazzUser>();

                XmlDocument xml = _accServ.getXml(_admins_url);
                XmlNamespaceManager nsMgr = _accServ.buildNamespaceManager(xml, new string[] { JazzConstants.jazzProcNum });
                nsMgr.AddNamespace(JazzConstants.jazzProc, JazzConstants.jazzProcNsUrl);

                string teamAreaXPath = "//" + JazzConstants.jazzProc + ":admin";
                XmlNodeList nodeList = xml.SelectNodes(teamAreaXPath, nsMgr);
                foreach (XmlNode n in nodeList)
                {
                    string currXPath = "./" + JazzConstants.jazzProc + ":user-url";
                    XmlNode currNode = n.SelectSingleNode(currXPath, nsMgr);
                    //_adm_list.Add(new JazzUser(currNode.InnerText, _accServ));
                    _adm_list.Add(JazzUserRepository.GetJazzUserRepository().GetOrAddUser(currNode.InnerText, _accServ));
                }
            }
            if (_adm_list != null)
                for (int i = 0; i < _adm_list.Count; i++)
                    yield return _adm_list[i];
        }

        private void GetMembers()
        {
            _mem_list = new List<JazzMember>();

            XmlDocument xml = _accServ.getXml(_members_url);
            XmlNamespaceManager nsMgr = _accServ.buildNamespaceManager(xml, new string[] { JazzConstants.jazzProcNum });
            //nsMgr.AddNamespace(JazzConstants.jazzProc, JazzConstants.jazzProcNsUrl);

            string teamAreaXPath = "//" + JazzConstants.jazzProcNum + ":member";
            XmlNodeList nodeList = xml.SelectNodes(teamAreaXPath, nsMgr);
            foreach (XmlNode n in nodeList)
            {
                string currXPath = "./" + JazzConstants.jazzProcNum + ":url";
                XmlNode currNode = n.SelectSingleNode(currXPath, nsMgr);
                _mem_list.Add(new JazzMember(currNode.InnerText, _accServ));
            }
        }

        public System.Collections.IEnumerable JazzProjectMembers()
        {
            if (_mem_list == null)
            {
                GetMembers();
            }
            if (_mem_list != null)
                for (int i = 0; i < _mem_list.Count; i++)
                    yield return _mem_list[i];
        }

        public bool HasMemberWithName(string MemberName)
        {
            if (_mem_list == null)
            {
                GetMembers();
            }
            if (_mem_list != null)
                for (int i = 0; i < _mem_list.Count; i++)
                    if (_mem_list[i].JazzMemberUser.Name.Equals(MemberName, StringComparison.OrdinalIgnoreCase))
                        return true;
            return false;
        }

        public JazzMember GetMemberWithName(string MemberName)
        {
            if (_mem_list == null)
                GetMembers();

            if (_mem_list != null)
                for (int i = 0; i < _mem_list.Count; i++)
                    if (_mem_list[i].JazzMemberUser.Name.Equals(MemberName, StringComparison.OrdinalIgnoreCase))
                        return _mem_list[i];
            return null;
        }

        public void DeleteMemberWithName(string MemberName)
        {
            if (_mem_list == null)
                GetMembers();

            if (_mem_list != null)
                for (int i = 0; i < _mem_list.Count; i++)
                    if (_mem_list[i].JazzMemberUser.Name.Equals(MemberName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (_accServ.delete(_mem_list[i].URL) == System.Net.HttpStatusCode.OK)
                            _mem_list.RemoveAt(i);
                        return;
                    }
        }

        public void AddMemberPending(JazzMember member)
        {
            member.PostIsPending = true;
            _mem_list.Add(member);
        }

        public void PostNewMembers()
        {
            XmlDocument px = new XmlDocument();

            px.InsertBefore(px.CreateXmlDeclaration("1.0", "UTF-8", null), px.DocumentElement);
            XmlElement rootNode = px.CreateElement(JazzConstants.jazzProcNum + ":members", JazzConstants.jazzProcNsUrl06);
            px.AppendChild(rootNode);
            int memPostCount = 0;
            foreach (JazzMember jm in JazzProjectMembers())
            {
                if (jm.PostIsPending)
                {
                    memPostCount++;
                    XmlElement mem = px.CreateElement(JazzConstants.jazzProcNum + ":member", JazzConstants.jazzProcNsUrl06);
                    px.DocumentElement.AppendChild(mem);
                    //XmlText usrCont = px.CreateTextNode(_url + "/members/" + jm.JazzMemberUser.Name);
                    //XmlElement usr = px.CreateElement(JazzConstants.jazzProcNum + ":usr", JazzConstants.jazzProcNsUrl);
                    //mem.AppendChild(usr);
                    //usr.AppendChild(usrCont);
                    XmlText usrUrlCont = px.CreateTextNode(jm.JazzMemberUser.URL);
                    XmlElement usrUrl = px.CreateElement(JazzConstants.jazzProcNum + ":user-url", JazzConstants.jazzProcNsUrl06);
                    mem.AppendChild(usrUrl);
                    usrUrl.AppendChild(usrUrlCont);
                    int memRoleCount = 0;
                    XmlElement roleAss = null;
                    foreach (JazzRole jr in jm.JazzMemberRoles())
                    {
                        if (memRoleCount == 0)
                        {
                            roleAss = px.CreateElement(JazzConstants.jazzProcNum + ":role-assignments", JazzConstants.jazzProcNsUrl06);
                            mem.AppendChild(roleAss);
                        }
                        XmlElement roleAs = px.CreateElement(JazzConstants.jazzProcNum + ":role-assignment", JazzConstants.jazzProcNsUrl06);
                        XmlText roleUrlCont = px.CreateTextNode(jr.URL);
                        XmlElement roleUrl = px.CreateElement(JazzConstants.jazzProcNum + ":role-url", JazzConstants.jazzProcNsUrl06);
                        roleAss.AppendChild(roleAs);
                        roleAs.AppendChild(roleUrl);
                        roleUrl.AppendChild(roleUrlCont);
                        memRoleCount++;
                    }
                    jm.PostIsPending = false;
                }
            }
            if (memPostCount > 0)
                _accServ.postXml(_url + "/members", px);
        }

        public void AddMember(JazzMember member)
        {
            XmlDocument px = new XmlDocument();

            px.InsertBefore(px.CreateXmlDeclaration("1.0", "UTF-8", null), px.DocumentElement);
            XmlElement rootNode = px.CreateElement(JazzConstants.jazzProcNum + ":members", JazzConstants.jazzProcNsUrl06);
            px.AppendChild(rootNode);
            XmlElement mem = px.CreateElement(JazzConstants.jazzProcNum + ":member", JazzConstants.jazzProcNsUrl06);
            px.DocumentElement.AppendChild(mem);
            XmlText usrUrlCont = px.CreateTextNode(member.JazzMemberUser.URL);
            XmlElement usrUrl = px.CreateElement(JazzConstants.jazzProcNum + ":user-url", JazzConstants.jazzProcNsUrl06);
            mem.AppendChild(usrUrl);
            usrUrl.AppendChild(usrUrlCont);

            string mem_url;
            _accServ.postXml(_url + "/members", px, out mem_url);
            member.SetMemberURL(mem_url);

            _mem_list.Add(member);
        }

        public System.Collections.IEnumerable JazzProjectRoles()
        {
            if (_role_list == null)
            {
                _role_list = new List<JazzRole>();

                XmlDocument xml = _accServ.getXml(_roles_url);
                XmlNamespaceManager nsMgr = _accServ.buildNamespaceManager(xml, new string[] { JazzConstants.jazzProcNum });
                //nsMgr.AddNamespace(JazzConstants.jazzProc, JazzConstants.jazzProcNsUrl);

                string teamAreaXPath = "//" + JazzConstants.jazzProcNum + ":role";
                XmlNodeList nodeList = xml.SelectNodes(teamAreaXPath, nsMgr);
                foreach (XmlNode n in nodeList)
                {
                    string currXPath = "./" + JazzConstants.jazzProcNum + ":url";
                    XmlNode currNode = n.SelectSingleNode(currXPath, nsMgr);
                    //_role_list.Add(new JazzRole(currNode.InnerText, _accServ));
                    _role_list.Add(JazzRoleCache.GetJazzRoleCache().GetOrAddRole(currNode.InnerText));
                }
            }
            if (_role_list != null)
                for (int i = 0; i < _role_list.Count; i++)
                    yield return _role_list[i];
        }

        public JazzRole GetJazzRoleByLabel(string role)
        {
            foreach (JazzRole jr in this.JazzProjectRoles())
                if (jr.Label == role)
                    return jr;
            return null;
        }
    }

    public class Jazz
    {
        private AccessRTCServer _accServ;
        private string _projAreasURL;
        public string ProjectAreasURL
        {
            get { return _projAreasURL; }
        }

        private List<JazzProjectArea> _pa_list = null;
        public enum repositoryType { ccm, rm }; 

        public Jazz(string repositoryURL, repositoryType rType, string username, string password)
        {
            JazzAccessRTCServer.SetAccessParams(repositoryURL, username, password);
            _accServ = JazzAccessRTCServer.GetJazzAccessRTCServer();

            XmlDocument xml;
            if (rType == repositoryType.ccm)
                xml = _accServ.getXml(repositoryURL + "/ccm/rootservices");
            else
                xml = _accServ.getXml(repositoryURL + "/rm/rootservices");

            //XmlNamespaceManager nsMgr = _accServ.buildNamespaceManager(xml, new string[] { "rdf" });
            //nsMgr.AddNamespace("oslc_cm", "http://open-services.net/xmlns/cm/1.0/");
            XmlNamespaceManager nsMgr = _accServ.buildNamespaceManager(xml, new string[] { "rdf", JazzConstants.jazzProcNum });

            //string projAreasXPath = "/rdf:Description/oslc_cm:cmServiceProviders/@rdf:resource";
            string currXPath = "/rdf:Description/" + JazzConstants.jazzProcNum + ":projectAreas/@rdf:resource";
            XmlNode node = xml.SelectSingleNode(currXPath, nsMgr);
            _projAreasURL = node.InnerText;
            //_projAreasURL = rm_root;
        }

        public System.Collections.IEnumerable JazzProjectAreas()
        {
            if (_pa_list == null)
            {
                _pa_list = new List<JazzProjectArea>();

                XmlDocument xml = _accServ.getXml(ProjectAreasURL + "?includeArchived=true");
                //nsMgr = _accServ.buildNamespaceManager(xml, new string[] { "rdf", "oslc", "dcterms" });
                XmlNamespaceManager nsMgr = _accServ.buildNamespaceManager(xml, new string[] { JazzConstants.jazzProcNum });

                //currXPath = "//oslc:ServiceProvider";
                string currXPath = "/" + JazzConstants.jazzProcNum + ":project-areas/" + JazzConstants.jazzProcNum + ":project-area";
                XmlNodeList nodeList = xml.SelectNodes(currXPath, nsMgr);
                foreach (XmlNode n in nodeList)
                {
                    //string paXPath = "//oslc:ServiceProvider[dcterms:title=\"" + node.InnerText + "\"]/oslc:details/@rdf:resource";
                    //string paXPath = "./oslc:details/@rdf:resource";
                    //XmlNode titleNode = n.SelectSingleNode(paXPath, nsMgr);

                    //currXPath = "./@" + JazzConstants.jazzProcNum + ":name";
                    //XmlNode currNode = n.SelectSingleNode(currXPath, nsMgr);
                    //string pa_name = currNode.InnerText;
                    //currXPath = "./" + JazzConstants.jazzProcNum + ":url";
                    //currNode = n.SelectSingleNode(currXPath, nsMgr);
                    //_pa_list.Add(new JazzProjectArea(pa_name, currNode.InnerText, _accServ));
                    _pa_list.Add(new JazzProjectArea(n, _accServ));
                }
            }
            if (_pa_list != null)
                for (int i = 0; i < _pa_list.Count; i++)
                    yield return _pa_list[i];
        }

        public JazzProjectArea GetJazzProjectAreaByName(string name)
        {
            if (_pa_list == null)
            {
                XmlDocument xml = _accServ.getXml(ProjectAreasURL + "?includeArchived=true");
                //nsMgr = _accServ.buildNamespaceManager(xml, new string[] { "rdf", "oslc", "dcterms" });
                XmlNamespaceManager nsMgr = _accServ.buildNamespaceManager(xml, new string[] { JazzConstants.jazzProcNum });

                //currXPath = "//oslc:ServiceProvider";
                string currXPath = "/" + JazzConstants.jazzProcNum + ":project-areas/" + JazzConstants.jazzProcNum + ":project-area[@" + JazzConstants.jazzProcNum + ":name='" + name + "']";
                XmlNode n = xml.SelectSingleNode(currXPath, nsMgr);
                if (n != null)
                    return new JazzProjectArea(n, _accServ);
            }
            else
                for (int i = 0; i < _pa_list.Count; i++)
                    if (_pa_list[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        return _pa_list[i];
            return null;
        }
    }
}
