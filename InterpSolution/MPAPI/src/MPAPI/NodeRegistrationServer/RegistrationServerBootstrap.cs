/*****************************************************************
 * MPAPI - Message Passing API
 * A framework for writing parallel and distributed applications
 * 
 * Author   : Frank Thomsen
 * Web      : http://sector0.dk
 * Contact  : mpapi@sector0.dk
 * License  : New BSD licence
 * 
 * Copyright (c) 2008, Frank Thomsen
 * 
 * Feel free to contact me with bugs and ideas.
 *****************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using RemotingLite;
using System.Net;

namespace MPAPI.RegistrationServer
{
    public class RegistrationServerBootstrap : IDisposable
    {
        private ServiceHost _host;
        private RegistrationServer _registrationServer;

        public void Open(int port)
        {
            Log.LogLevel = LogLevel.InfoWarningError;
            Log.LogType = LogType.Console;
            _registrationServer = new RegistrationServer();

            _host = new ServiceHost(_registrationServer, port);
            _host.Open();
            Console.Title = "Registration server";
            Log.Info("Registration server is running.");
            Log.Info($"IsIPv6LinkLocal = {_host.EndPoint.Address.IsIPv6LinkLocal}, IsIPv6Multicast = {_host.EndPoint.Address.IsIPv6Multicast}  IsIPv6SiteLocal = {_host.EndPoint.Address.IsIPv6SiteLocal}");
            Log.Info($"Address = {_host.EndPoint.Address.AddressFamily.ToString()}");

            var ips = Dns.GetHostEntry(_host.EndPoint.Address).AddressList;
            int i = 0;
            foreach (var ip in ips)
            {
                Log.Info($"ip[{i++}] : {ip.ToString()}");
            }
            Log.Info($"potr : {port}");
            string input = "";
            do {
                input = Console.ReadLine();
                switch (input) {
                    case "scount":
                        Console.WriteLine($"Slaves count : {_registrationServer.GetAllNodeEndPoints().Count}");
                        break;
                    case "slaves":
                        i = 0;
                        foreach (var s in _registrationServer.GetAllNodeEndPoints()) {
                            Console.WriteLine($"[{i++}] : {s.ToString()}");
                        }       
                        break;
                    default:
                        Console.WriteLine("Такой команды нет (exit для выхода если что)");
                        break;
                }
            } while (input!="exit");
            //Console.ReadLine();
            Log.Info("Registration server terminated.");
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_host != null)
                _host.Dispose();

            _registrationServer.Dispose();
        }

        #endregion
    }
}
