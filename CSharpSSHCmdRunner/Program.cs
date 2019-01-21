using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace CSharpSSHCmdRunner
{
    class Program
    {
        private static readonly string _Version = "20190121";
        private static readonly string _LogTxt = "执行报告.txt";
        static void WriteLogLine(string line)
        {
            using (var sw = new StreamWriter(_LogTxt, true))
            {
                Console.WriteLine(line);
                sw.WriteLine(line);
            }
        }
        /// <summary>读取文件，返回一个含有文件数据的行列表</summary>
        /// <param name="TxtFilePath">文件路径</param>
        /// <returns>文件数据的行列表</returns>
        private static List<string> ReadTxtFromFile(string TxtFilePath)
        {
            // 1 首先创建一个泛型为string 的List列表
            List<string> allRowStrList = new List<string>();
            {
                // 2 加载文件
                System.IO.StreamReader sr = new
                    System.IO.StreamReader(TxtFilePath,
                        Encoding.Default);
                String line; // 3 调用StreamReader的ReadLine()函数
                while ((line = sr.ReadLine()) != null)
                {   // 4 将每行添加到文件List中
                    allRowStrList.Add(line);
                }
                // 5 关闭流
                sr.Close();
            }
            // 6 完成操作
            return allRowStrList;
        }

        static void Main(string[] args)
        {
            WriteLogLine("RunnerVer: " + _Version);

            if (args.Length == 0)
            {
                //System.Console.WriteLine("Please enter a numeric argument.");
                //return;
            }
            else
            {
                foreach (var arg in args)
                {
                    if (arg == "-h" || arg == "-help" || arg == "h" || arg == "help")
                    {
                        Console.WriteLine("About:");
                        Console.WriteLine("本程序用于自动化执行ssh命令，首先从SSHConnectionInfo.xml中读取ssh连接信息，" +
                                          "而后从SSH_CMD.txt按行执行其中的指令。执行过程中只要有任意指令出错，则结束整个执行过程。");
                    }
                }
            }

            if (File.Exists(_LogTxt))
            {
                File.Delete(_LogTxt);
            }


            try
            {
                // 读取连接配置文件
                SSHConnectionInfo sshci = new SSHConnectionInfo();
                if (File.Exists("SSHConnectionInfo.xml"))
                {
                    try
                    {
                        sshci = sshci.CreateFromXmlFile("SSHConnectionInfo.xml");
                    }
                    catch (Exception e)
                    {
                        sshci = new SSHConnectionInfo();
                        sshci.ToXmlFile("SSHConnectionInfo.xml");
                    }
                }
                else
                {
                    sshci.ToXmlFile("SSHConnectionInfo.xml");
                }

                if (string.IsNullOrEmpty(sshci.IpArrress) ||
                    !int.TryParse(sshci.IpArrress.Replace(".", ""), out var tmpIp))
                {
                    throw new Exception("尚未配置SSH连接信息，请修改SSHConnectionInfo.xml");
                }

                // 读取待执行命令
                if (File.Exists("SSH_CMD.txt"))
                {
                    // 五分类算法测试用例
                    var cmds = ReadTxtFromFile("SSH_CMD.txt");
                    if (cmds.Count > 0)
                    {
                        // 执行
                        RunSshCommands("172.20.65.72", 18001, "shawn", "123456", cmds.ToArray());
                        return;
                    }
                }
                else
                {
                    File.Create("SSH_CMD.txt");
                }
                throw new Exception("无可执行命令");
            }
            catch (Exception e)
            {
                WriteLogLine(e.Message);
                Console.WriteLine(e);
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// SSH登录远程Linux服务器，并运行指令
        /// </summary>
        /// <param name="host">远程Linux服务器IP或域名</param>
        /// <param name="port"></param>
        /// <param name="username">账号名</param>
        /// <param name="password">账号密码</param>
        /// <param name="commands">命令</param>
        /// <returns></returns>
        public static bool RunSshCommands(string host, int port, string username, string password, string[] commands)
        {
            if (commands == null || commands.Length == 0)
                return false;

            // 建立连接
            var connectionInfo = new ConnectionInfo(host, port,
                username, new AuthenticationMethod[]{
                    new PasswordAuthenticationMethod(username,password)
                });


            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                if (client.IsConnected)
                {
                    foreach (var cmd in commands)
                    {
                        WriteLogLine("[send]" + cmd + ":");
                        //执行命令
                        SshCommand sshCmd = client.RunCommand(cmd);
                        //WriteLogLine("开始遍历五分类算法测试用例" + DateTime.Now);
                        if (!string.IsNullOrWhiteSpace(sshCmd.Error))
                        {
                            if (sshCmd.ExitStatus != 0)
                            {
                                WriteLogLine("[ERROR]" + sshCmd.Error);
                                throw new Exception(sshCmd.Error);
                            }
                            else
                            {
                                WriteLogLine("[WARNING]" + sshCmd.Error);
                            }
                        }
                        WriteLogLine("[receive]" + sshCmd.Result);
                    }
                }
            }



            return true;
        }
    }
}
