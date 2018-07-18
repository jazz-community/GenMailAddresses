using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RuS.Jazz;

namespace GenMailAddresses.Test2
{
    //[SetUpFixture]
    public class JazzConnectionParams
    {
        public string Repository { get; set; }
        public string User { get; set; }
        public string PW { get; set; }

        //[SetUp]
        public JazzConnectionParams()
        {
            Repository = "https://jazzdev.rsint.net";
            //User = "cc-tfs-syncmgr";
            //PW = "test123+^#";
            User = "CAXMGR";
            PW = "neu-ulm1";
        }
    }

    [TestFixture]
    public class JazzTests
    {
        [Test]
        public void T21_Jazz()
        {
            JazzConnectionParams jcp = new JazzConnectionParams();
            Jazz jazz = new Jazz(jcp.Repository, RuS.Jazz.Jazz.repositoryType.ccm, jcp.User, jcp.PW);
            Assert.That(jazz.ProjectAreasURL.Equals(jcp.Repository + "/ccm/process/project-areas"));
        }

        [Test]
        //[ExpectedException(typeof(RuS.Jazz.AuthenticationException))]
        public void T22_Jazz()
        {
            JazzConnectionParams jcp = new JazzConnectionParams();
            Jazz jazz = new Jazz(jcp.Repository, RuS.Jazz.Jazz.repositoryType.ccm, jcp.User, jcp.PW + "x");
            //JazzProjectArea pa = jazz.GetJazzProjectAreaByName("xxx");
            Assert.That(() => jazz.GetJazzProjectAreaByName("xxx"),
               Throws.TypeOf<RuS.Jazz.AuthenticationException>());
        }

        [Test]
        public void T23_Jazz()
        {
            JazzConnectionParams jcp = new JazzConnectionParams();
            Jazz jazz = new Jazz(jcp.Repository, RuS.Jazz.Jazz.repositoryType.ccm, jcp.User, jcp.PW);
            JazzProjectArea pa = jazz.GetJazzProjectAreaByName("xxx");
            Assert.IsNull(pa);
        }

        [Test]
        public void T24_Jazz()
        {
            JazzConnectionParams jcp = new JazzConnectionParams();
            Jazz jazz = new Jazz(jcp.Repository, RuS.Jazz.Jazz.repositoryType.ccm, jcp.User, jcp.PW);
            int i = 0;
            foreach (JazzProjectArea p_a in jazz.JazzProjectAreas())
                i++;
            Assert.That(i > 1);
            JazzProjectArea pa = jazz.GetJazzProjectAreaByName("PLM-helmerichs_play");
            Assert.IsNotNull(pa);
        }

        [Test]
        public void T25_Jazz()
        {
            JazzConnectionParams jcp = new JazzConnectionParams();
            Jazz jazz = new Jazz(jcp.Repository, RuS.Jazz.Jazz.repositoryType.ccm, jcp.User, jcp.PW);
            JazzProjectArea pa = jazz.GetJazzProjectAreaByName("PLM-helmerichs_play");
            Assert.IsNotNull(pa);
            foreach (JazzTeamArea ta in pa.JazzTeamAreas())
            {
                JazzTeamArea ta2 = pa.GetJazzTeamAreaByName(ta.Name);
                Assert.IsNotNull(ta2);
                Assert.That(ta.Name == ta2.Name);
            }
        }

        [Test]
        public void T26_Jazz()
        {
            JazzConnectionParams jcp = new JazzConnectionParams();
            Jazz jazz = new Jazz(jcp.Repository, RuS.Jazz.Jazz.repositoryType.ccm, jcp.User, jcp.PW);
            JazzProjectArea pa = jazz.GetJazzProjectAreaByName("PLM-helmerichs_play");
            Assert.IsNotNull(pa);
            JazzTeamArea s_ta = pa.GetJazzTeamAreaByName("Helmerichs Team 1");
            Assert.IsNotNull(s_ta);
            JazzTeamArea t_ta = pa.GetJazzTeamAreaByName("RolTeam 2 Subteam 1");
            Assert.IsNotNull(t_ta);
        }

        [Test]
        public void T27_Jazz()
        {
            JazzConnectionParams jcp = new JazzConnectionParams();
            Jazz jazz = new Jazz(jcp.Repository, RuS.Jazz.Jazz.repositoryType.ccm, jcp.User, jcp.PW);
            JazzProjectArea pa = jazz.GetJazzProjectAreaByName("PLM-helmerichs_play");
            Assert.IsNotNull(pa);
            JazzTeamArea s_ta = pa.GetJazzTeamAreaByName("Helmerichs Team 1");
            Assert.IsNotNull(s_ta);
            JazzTeamArea t_ta = pa.GetJazzTeamAreaByName("RolTeam 2 Subteam 1");
            Assert.IsNotNull(t_ta);
            foreach (JazzMember m in s_ta.JazzTeamMembers())
            {
                if (!t_ta.HasMemberWithName(m.JazzMemberUser.Name))
                {
                    t_ta.AddMember(m);
                }
            }
            t_ta.PostNewMembers();
        }
    }
}
