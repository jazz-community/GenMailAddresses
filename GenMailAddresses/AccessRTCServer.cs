using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Web;

namespace RuS.Jazz
{
    public class AuthenticationException : Exception
    {
        public AuthenticationException() { }

        public AuthenticationException(string message)
            : base(message)
        { }

        public AuthenticationException(string message, Exception inner)
            : base(message, inner)
        { }
    }

    public class AccessRTCServer
    {
        private string _serverURL;
        public string RTCServerURL
        {
            set { _serverURL = value; }
        }
        private string _user;
        public string User
        {
            set { _user = value; }
        }
        private string _password;
        public string Password
        {
            set { _password = value; }
        }

        public AccessRTCServer()
        {
            _serverURL = string.Empty;
            _user = string.Empty;
            _password = string.Empty;
        }

        public AccessRTCServer(string serverURL, string userName, string passWord)
        {
            _serverURL = serverURL;
            _user = userName;
            _password = passWord;
        }

        private HttpWebResponse requestSecureDocument(HttpWebRequest _request, string _rtcServerURL)
        {
            //FormBasedAuth Step1: Request the resource and clone the request to be used later
            HttpWebRequest _requestClone = WebRequestExtensions.CloneRequest(_request, _request.RequestUri);
            //(HttpWebRequest)WebRequest.Create(request.RequestUri);

            HttpWebResponse _docResponse = null;
            try
            {
	            //store the response in _docResponse variable
	            _docResponse = (HttpWebResponse)_request.GetResponse();
            }
            catch (System.Exception)
            {
                // He 18-mar-2014: Exception is thrown instead of "authrequired" response when trying an unauthorized connect to the rm repository

                //Prepare form for authentication as _rtcAuthHeader = authrequired
                HttpWebRequest _formPost = (HttpWebRequest)WebRequest.Create(_rtcServerURL + "/rm/j_security_check");
                _formPost.Method = "POST";
                _formPost.Timeout = 30000;
                _formPost.CookieContainer = _request.CookieContainer;
                _formPost.Accept = "text/xml";
                _formPost.ContentType = "application/x-www-form-urlencoded";

                string _authString = "j_username=" + _user + "&j_password=" + HttpUtility.UrlEncode(_password); //create authentication string
                Byte[] _outBuffer = Encoding.UTF8.GetBytes(_authString); //store in byte buffer
                _formPost.ContentLength = _outBuffer.Length;
                Stream _str = _formPost.GetRequestStream();
                _str.Write(_outBuffer, 0, _outBuffer.Length); //update form
                _str.Close();

                //FormBasedAuth Step2:submit the login form and get the response from the server
                HttpWebResponse _formResponse = (HttpWebResponse)_formPost.GetResponse();

                string _rtcAuthHeader = _formResponse.Headers["X-com-ibm-team-repository-web-auth-msg"];
                //check if authentication has failed
                if ((_rtcAuthHeader != null) && _rtcAuthHeader.Equals("authfailed"))
                {
                    //authentication failed. You can write code to handle the authentication failure.
                    //if (DEBUG) Console.WriteLine("Authentication Failure");
                    throw new AuthenticationException("Authentication failure");
                }
                else
                {
                    //login successful
                    _formResponse.GetResponseStream().Flush();
                    _formResponse.Close();
                    //FormBasedAuth Step3: Resend the request for the protected resource.
                    //if (DEBUG) Console.WriteLine("&gt;&gt; Response " + request.RequestUri);
                    return (HttpWebResponse)_requestClone.GetResponse();
                }            	
            }

            //HttpStatusCode.OK indicates that the request succeeded and that the requested information is in the response.
            if (_docResponse.StatusCode == HttpStatusCode.OK)
            {
                //X-com-ibm-team-repository-web-auth-msg header signifies form based authentication is being used
                string _rtcAuthHeader = _docResponse.Headers["X-com-ibm-team-repository-web-auth-msg"];
                if ((_rtcAuthHeader != null) && _rtcAuthHeader.Equals("authrequired"))
                {
                    _docResponse.GetResponseStream().Flush();
                    _docResponse.Close();

                    //Prepare form for authentication as _rtcAuthHeader = authrequired
                    HttpWebRequest _formPost = (HttpWebRequest)WebRequest.Create(_rtcServerURL + "/ccm/j_security_check");
                    _formPost.Method = "POST";
                    _formPost.Timeout = 30000;
                    _formPost.CookieContainer = _request.CookieContainer;
                    _formPost.Accept = "text/xml";
                    _formPost.ContentType = "application/x-www-form-urlencoded";

                    string _authString = "j_username=" + _user + "&j_password=" + HttpUtility.UrlEncode(_password); //create authentication string
                    Byte[] _outBuffer = Encoding.UTF8.GetBytes(_authString); //store in byte buffer
                    _formPost.ContentLength = _outBuffer.Length;
                    Stream _str = _formPost.GetRequestStream();
                    _str.Write(_outBuffer, 0, _outBuffer.Length); //update form
                    _str.Close();

                    //FormBasedAuth Step2:submit the login form and get the response from the server
                    HttpWebResponse _formResponse = (HttpWebResponse)_formPost.GetResponse();

                    _rtcAuthHeader = _formResponse.Headers["X-com-ibm-team-repository-web-auth-msg"];
                    //check if authentication has failed
                    if ((_rtcAuthHeader != null) && _rtcAuthHeader.Equals("authfailed"))
                    {
                        //authentication failed. You can write code to handle the authentication failure.
                        //if (DEBUG) Console.WriteLine("Authentication Failure");
                        throw new AuthenticationException("Authentication failure");
                    }
                    else
                    {
                        //login successful
                        _formResponse.GetResponseStream().Flush();
                        _formResponse.Close();
                        //FormBasedAuth Step3: Resend the request for the protected resource.
                        //if (DEBUG) Console.WriteLine("&gt;&gt; Response " + request.RequestUri);
                        return (HttpWebResponse)_requestClone.GetResponse();
                    }
                }
            }
            //already authenticated return original response_docResponse
            return _docResponse;
        }

        private CookieContainer _cookieCont = new CookieContainer();

        public XmlDocument getXml(string requestURI, bool acceptRdf = false)
        {
            HttpWebRequest httpclient = (HttpWebRequest)WebRequest.Create(requestURI);
            httpclient.Method = "GET";
            if (acceptRdf)
                httpclient.Accept = "application/rdf+xml";
            else
                httpclient.Accept = "text/xml";
            httpclient.Headers.Set("OSLC-Core-Version", "2.0");
            httpclient.CookieContainer = _cookieCont;

            HttpWebResponse response = requestSecureDocument(httpclient, _serverURL);
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string s = reader.ReadToEnd();
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(s);
            responseStream.Close();
            reader.Close();
            response.Close();
            return xml;
        }

        public HttpStatusCode postXml(string requestURI, XmlDocument xmlToPost)
        {
            HttpWebRequest httpclient = (HttpWebRequest)WebRequest.Create(requestURI);
            httpclient.Method = "POST";
            httpclient.Timeout = 30000;
            httpclient.CookieContainer = _cookieCont;
            httpclient.Accept = "text/xml";
            httpclient.Headers.Set("OSLC-Core-Version", "2.0");
            Uri u = new Uri(requestURI);
            CookieCollection cc = _cookieCont.GetCookies(u);
            string s_id = string.Empty;
            foreach (Cookie c in cc)
            {
                if (c.Name == "JSESSIONID")
                {
                    s_id = c.Value;
                    break;
                }
            }
            httpclient.Headers.Set("X-Jazz-CSRF-Prevent", s_id);
            //httpclient.ContentType = "application/x-www-form-urlencoded";

            Stream _str = httpclient.GetRequestStream();
            xmlToPost.Save(_str);
            //xmlToPost.Save("C:\\tmp\\test.txt");
            _str.Close();

            //HttpWebResponse response = (HttpWebResponse)httpclient.GetResponse();
            HttpWebResponse response = requestSecureDocument(httpclient, _serverURL);
            return response.StatusCode;
        }

        public HttpStatusCode postXml(string requestURI, XmlDocument xmlToPost, out string responseURI)
        {
            HttpWebRequest httpclient = (HttpWebRequest)WebRequest.Create(requestURI);
            httpclient.Method = "POST";
            httpclient.Timeout = 30000;
            httpclient.CookieContainer = _cookieCont;
            httpclient.Accept = "text/xml";
            httpclient.Headers.Set("OSLC-Core-Version", "2.0");
            Uri u = new Uri(requestURI);
            CookieCollection cc = _cookieCont.GetCookies(u);
            string s_id = string.Empty;
            foreach (Cookie c in cc)
            {
                if (c.Name == "JSESSIONID")
                {
                    s_id = c.Value;
                    break;
                }
            }
            httpclient.Headers.Set("X-Jazz-CSRF-Prevent", s_id);
            //httpclient.ContentType = "application/x-www-form-urlencoded";

            Stream _str = httpclient.GetRequestStream();
            xmlToPost.Save(_str);
            //xmlToPost.Save("C:\\tmp\\test.txt");
            _str.Close();

            //HttpWebResponse response = (HttpWebResponse)httpclient.GetResponse();
            HttpWebResponse response = requestSecureDocument(httpclient, _serverURL);
            responseURI = response.Headers["Location"];
            return response.StatusCode;
        }

        public HttpStatusCode putXml(string requestURI, XmlDocument xmlToPut)
        {
            HttpWebRequest httpclient = (HttpWebRequest)WebRequest.Create(requestURI);
            httpclient.Method = "PUT";
            httpclient.Timeout = 30000;
            httpclient.CookieContainer = _cookieCont;
            httpclient.Accept = "text/xml";
            httpclient.Headers.Set("OSLC-Core-Version", "2.0");
            Uri u = new Uri(requestURI);
            CookieCollection cc = _cookieCont.GetCookies(u);
            string s_id = string.Empty;
            foreach (Cookie c in cc)
            {
                if (c.Name == "JSESSIONID")
                {
                    s_id = c.Value;
                    break;
                }
            }
            httpclient.Headers.Set("X-Jazz-CSRF-Prevent", s_id);
            //httpclient.ContentType = "application/x-www-form-urlencoded";

            Stream _str = httpclient.GetRequestStream();
            xmlToPut.Save(_str);
            //xmlToPost.Save("C:\\tmp\\test.txt");
            _str.Close();

            //HttpWebResponse response = (HttpWebResponse)httpclient.GetResponse();
            HttpWebResponse response = requestSecureDocument(httpclient, _serverURL);
            return response.StatusCode;
        }

        public HttpStatusCode delete(string requestURI)
        {
            HttpWebRequest httpclient = (HttpWebRequest)WebRequest.Create(requestURI);
            httpclient.Method = "DELETE";
            httpclient.Accept = "text/xml";
            httpclient.Headers.Set("OSLC-Core-Version", "2.0");
            httpclient.CookieContainer = _cookieCont;
            Uri u = new Uri(requestURI);
            CookieCollection cc = _cookieCont.GetCookies(u);
            string s_id = string.Empty;
            foreach (Cookie c in cc)
            {
                if (c.Name == "JSESSIONID")
                {
                    s_id = c.Value;
                    break;
                }
            }
            httpclient.Headers.Set("X-Jazz-CSRF-Prevent", s_id);

            HttpWebResponse response = requestSecureDocument(httpclient, _serverURL);
            return response.StatusCode;
        }

        public XmlNamespaceManager buildNamespaceManager(XmlDocument _xml, string[] _nsprefixes)
        {
            XmlNameTable nt = new NameTable();
            XmlNamespaceManager n = new XmlNamespaceManager(nt);
            foreach (string prefix in _nsprefixes)
            {
                string nsStr = _xml.LastChild.GetNamespaceOfPrefix(prefix);
                n.AddNamespace(prefix, nsStr);
            }
            return n;
        }

        public XmlNamespaceManager buildNamespaceManager(XmlNode _node, string[] _nsprefixes)
        {
            XmlNameTable nt = new NameTable();
            XmlNamespaceManager n = new XmlNamespaceManager(nt);
            foreach (string prefix in _nsprefixes)
            {
                string nsStr = _node.GetNamespaceOfPrefix(prefix);
                n.AddNamespace(prefix, nsStr);
            }
            return n;
        }
    }
}
