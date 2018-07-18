using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RuS.Jazz;
using System.IO;

namespace GenMailAddresses
{
    class Program
    {
        static Options rtOptions;

        public class NotFoundException : Exception
        {
            public NotFoundException() { }

            public NotFoundException(string message)
                : base(message)
            { }

            public NotFoundException(string message, Exception inner)
                : base(message, inner)
            { }
        }

        class mailing_list
        {
            private Dictionary<string, string> _ml = null;

            public void AddMember(JazzMember member)
            {
                if (_ml == null)
                    _ml = new Dictionary<string, string>();

                if (!_ml.ContainsKey(member.JazzMemberUser.Name) && !_ml.ContainsValue(member.JazzMemberUser.MailAddress))
                {
                    bool addOk = true;
                    if (rtOptions.roles != null)
                    {
                        addOk = false;
                        for (int i = 0; i < rtOptions.roles.Count; i++)
                        {
                            if (rtOptions.roles[i] == string.Empty)
                            {
                                if (member.JazzMemberHasDefaultRoleOnly())
                                {
                                    addOk = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (member.JazzMemberHasRoleWithLabel(rtOptions.roles[i]))
                                {
                                    addOk = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (addOk && !member.JazzMemberUser.MailAddress.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                        _ml.Add(member.JazzMemberUser.Name, member.JazzMemberUser.MailAddress);
                }
            }

            public System.Collections.IEnumerable mailaddresses()
            {
                if (_ml != null)
                {
                    var list = _ml.Values.ToList();
                    list.Sort();
                    foreach (string v in list)
                        yield return v;
                }
            }
        }

        static void addSubTeamArea(mailing_list mailList, JazzTeamArea teamArea, string areaRoot)
        {
            if (rtOptions.opt_verbose)
                Console.WriteLine("Scanning (Sub-)TeamArea '{0}\\{1}'", areaRoot, teamArea.Name);
            foreach (JazzMember m in teamArea.JazzTeamMembers())
                mailList.AddMember(m);

            foreach (JazzTeamArea ta in teamArea.JazzTeamAreas())
                addSubTeamArea(mailList, ta, areaRoot + "\\" + teamArea.Name);
        }

        static string getUsername()
        {
            string usr = "";
            Console.Write("Username: ");
            usr = Console.ReadLine();
            return usr;
        }

        static string getPassword()
        {
            string pass = "";
            Console.Write("Password: ");
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter && key.KeyChar != '\0')
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();

            return pass;
        }

        static int Main(string[] args)
        {
            const string idstring = "@(#)GenMailAddresses 1.9 2018/04/20 R&S 6DSE";
            bool write_id = true;
            int exitCode = 0;
            StreamWriter sw = null;
            
            try
            {
                rtOptions = ArgumentsHandling.getOptionsFromArguments(args);
                if (rtOptions.opt_verbose)
                {
                    Console.WriteLine(idstring);
                    Console.WriteLine();
                    write_id = false;
                }
                if (rtOptions.outfile != null)
                    sw = new StreamWriter(rtOptions.outfile, false, Encoding.UTF8);

                if ((rtOptions.user == null) || (rtOptions.passwd == null))
                {
                    // He 7-mar-2014: Write idstring only if not already written due to verbose option
                    if (write_id)
                    {
                        Console.WriteLine(idstring);
                        Console.WriteLine();
                        write_id = false;
                    }
                    if (rtOptions.user == null)
                        rtOptions.user = getUsername();
                    if (rtOptions.passwd == null)
                        rtOptions.passwd = getPassword();
                    Console.WriteLine();
                }

                RuS.Jazz.Jazz.repositoryType repoType;
                if (rtOptions.opt_rm)
                    repoType = RuS.Jazz.Jazz.repositoryType.rm;
                else
                    repoType = RuS.Jazz.Jazz.repositoryType.ccm;

                Jazz jazz = new Jazz(rtOptions.jazzRepository, repoType, rtOptions.user, rtOptions.passwd);
                string projectArea = "<ALL AREAS>";
                mailing_list mlist = new mailing_list();

                if (rtOptions.opt_all_areas)
                {
                    foreach (JazzProjectArea pa in jazz.JazzProjectAreas())
                    {
                        if (pa.isArchived)
                            continue;

                        if (rtOptions.opt_verbose)
                            Console.WriteLine("Scanning ProjectArea '{0}'", pa.Name);
                        foreach (JazzMember m in pa.JazzProjectMembers())
                            mlist.AddMember(m);
                        if (rtOptions.opt_recursive)
                            foreach (JazzTeamArea s_ta in pa.JazzTeamAreas())
                            {
                                if (s_ta.isArchived)
                                    continue;
                                addSubTeamArea(mlist, s_ta, pa.Name);
                            }
                    }
                }
                else
                {
                    string[] a_arr = rtOptions.area.Split('\\');
                    projectArea = a_arr[0];

                    JazzProjectArea pa = jazz.GetJazzProjectAreaByName(projectArea);
                    if (pa == null)
                        throw new NotFoundException("ProjectArea '" + projectArea + "' not found");

                    JazzTeamArea ta = null;
                    for (int i = 1; i < a_arr.Length; i++)
                    {
                        if (ta == null)
                            ta = pa.GetJazzTeamAreaByName(a_arr[i]);
                        else
                            ta = ta.GetJazzSubTeamAreaByName(a_arr[i]);

                        if (ta == null)
                            throw new NotFoundException("TeamArea '" + a_arr[i] + "' not found within ProjectArea '" + projectArea + "'");
                    }
                    if (ta == null)
                    {
                        if (rtOptions.opt_verbose)
                            Console.WriteLine("Scanning ProjectArea '{0}'", projectArea);
                        foreach (JazzMember m in pa.JazzProjectMembers())
                            mlist.AddMember(m);
                        if (rtOptions.opt_recursive)
                            foreach (JazzTeamArea s_ta in pa.JazzTeamAreas())
                            {
                                if (s_ta.isArchived)
                                    continue;
                                addSubTeamArea(mlist, s_ta, projectArea);
                            }
                    }
                    else
                    {
                        if (rtOptions.opt_verbose)
                            Console.WriteLine("Scanning TeamArea '{0}\\{1}'", projectArea, ta.Name);
                        foreach (JazzMember m in ta.JazzTeamMembers())
                            mlist.AddMember(m);
                        if (rtOptions.opt_recursive)
                            foreach (JazzTeamArea s_ta in ta.JazzTeamAreas())
                            {
                                if (s_ta.isArchived)
                                    continue;
                                addSubTeamArea(mlist, s_ta, projectArea + "\\" + ta.Name);
                            }
                    }
                }

                if (rtOptions.opt_verbose)
                {
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("Resulting mail addresses:");
                    Console.WriteLine("-------------------------");
                }
                if (sw != null)
                {
                    sw.WriteLine(idstring);
                    //sw.WriteLine();
                    //sw.WriteLine(DateTime.Now.ToString());
                    sw.WriteLine();
                    sw.WriteLine("Mail addresses as of {0:r} for members of", DateTime.Now);
                    sw.WriteLine("  Jazz server : {0}", rtOptions.jazzRepository);
                    sw.WriteLine("  Account used: {0}", rtOptions.user);
                    sw.WriteLine("  Project area: {0}", projectArea);
                    if (!rtOptions.opt_all_areas)
                        if (rtOptions.area.Length > (projectArea.Length + 1))
                            sw.WriteLine("  Team area   : {0}", rtOptions.area.Replace(projectArea + "\\", string.Empty));
                    if (rtOptions.opt_recursive)
                    {
                        sw.WriteLine();
                        sw.WriteLine("  Recursive mode ON: Members of all subordinate team areas are included.");
                    }
                    if (rtOptions.roles != null)
                    {
                        sw.WriteLine();
                        sw.WriteLine("  Mail addresses are restricted to members with the following roles:");
                        for (int i = 0; i < rtOptions.roles.Count; i++ )
                        {
                            if (rtOptions.roles[i] == string.Empty)
                                sw.WriteLine("    <NO ROLE>");
                            else
                                sw.WriteLine("    {0}", rtOptions.roles[i]);
                        }
                    }
                    sw.WriteLine();
                }

                string mailaddr_out = string.Empty;
                int mailaddr_count = 0;
                int mailaddr_block_count = 0;
                foreach (string ma in mlist.mailaddresses())
                {
                    string ma_l = ma.ToLower();
                    if (ma_l.Contains("@nop.rsint.net"))
                        continue;
                    mailaddr_count++;
                    mailaddr_block_count++;
                    if (mailaddr_block_count > 500)
                    {
                        Console.WriteLine(mailaddr_out + "\n");
                        if (sw != null)
                        {
                            sw.WriteLine(mailaddr_out);
                            sw.WriteLine();
                        }
                        mailaddr_out = string.Empty;
                        mailaddr_block_count = 0;
                    }
                    if (mailaddr_out != string.Empty)
                        mailaddr_out += ";";
                    mailaddr_out += ma_l;
                }
                if (mailaddr_count == 0)
                {
                    Console.WriteLine("<MAIL ADDRESS LIST EMPTY: No matching members found>");
                    if (sw != null)
                        sw.WriteLine("<MAIL ADDRESS LIST EMPTY: No matching members found>");
                }
                else
                {
                    Console.WriteLine(mailaddr_out);
                    if (sw != null)
                        sw.WriteLine(mailaddr_out);
                }
            }
            catch (RuS.Jazz.AuthenticationException ex)
            {
                if (write_id && !rtOptions.opt_verbose)
                {
                    Console.WriteLine(idstring);
                    Console.WriteLine();
                }
                Console.WriteLine("Exception occurred, message: {0}\n", ex.Message);
                exitCode = 3;
            }
            catch (NotFoundException ex)
            {
                if (write_id && !rtOptions.opt_verbose)
                {
                    Console.WriteLine(idstring);
                    Console.WriteLine();
                }
                Console.WriteLine("Exception occurred, message: {0}\n", ex.Message);
                exitCode = 4;
            }
            catch (NoArgumentsException)
            {
                Console.WriteLine(idstring);
                Console.WriteLine();
                Console.WriteLine(ArgumentsHandling.getUsageMessage("GenMailAddresses"));
                exitCode = 1;
            }
            catch (ArgumentsParseException ex)
            {
                Console.WriteLine(idstring);
                Console.WriteLine(); 
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine(ArgumentsHandling.getUsageMessage("GenMailAddresses"));
                exitCode = 1;
            }
            catch (Exception ex)
            {
                if (write_id && !rtOptions.opt_verbose)
                {
                    Console.WriteLine(idstring);
                    Console.WriteLine();
                }
                Console.WriteLine("Exception occurred, message: {0}\n", ex.Message);
                exitCode = 9;
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }
            return exitCode;
        }
    }
}
