
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System;



// using Unity.Services.Relay.Models;
// using Unity.Services.Relay;
// using System.Text;
public enum Type
{
    None,
    Host,
    Client,
    Server

}
public class TransportController : MonoBehaviour
{
    public static TransportController instance { get; private set; }
    [SerializeField] GameObject loadingPanel;
    public RawImage qrDisplay;
    public GameObject networkUIPanel;
    private const string URL = "https://lg---livescore.web.app/?code=";

#if UNITY_WEBGL && !UNITY_EDITOR
   [DllImport("__Internal")]
    private static extern string GetUrlParam(string param);
#endif


    public Type type = Type.Server;

    public Text ipText, relayCodeText;
    public Text networkIpText;
    private string ipAddress;
    private ushort port = 7777;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }





    async void Start()
    {
        // #if UNITY_WEBGL && !UNITY_EDITOR
        //              type = Type.Client;
        // #endif

        try
        {
            SetLoadingActive(true, "Initializing...");
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("✅ Signed in as Player: " + AuthenticationService.Instance.PlayerId);
            }
        }
        catch (AuthenticationException e)
        {
            Debug.LogError("❌ Player Authentication failed: " + e.Message);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Player Authentication failed: " + e.Message);
        }

        SetLoadingActive(false);


        Debug.Log("Unity Services initialized and signed in!");
        // Create allocation for 1 host + N clients
        // if (type == Type.Server)
        // {
        // Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3); // max 3 clients

        // string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        // Debug.Log("Relay join code: " + joinCode);

        // // Configure Unity Transport
        // var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        // var serverData = AllocationUtils.ToRelayServerData(allocation, "wss");
        // transport.SetRelayServerData(serverData);
        // NetworkManager.Singleton.StartServer();
        // loadingPanel.SetActive(false);
        // qrDisplay.texture = QRCodeUnity.GenerateQR(URL + joinCode, 256, 256);
        // relayCodeText.text = "Code: " + joinCode;
        //}
        // else if (type == Type.Client)
        // {

#if UNITY_WEBGL && !UNITY_EDITOR
        string joinCode = "";
                    joinCode = GetUrlParam("code");
                     print("join code form param : " + joinCode);

        if(string.IsNullOrEmpty(joinCode)) return;
            StartClient(joinCode);
            networkUIPanel.SetActive(false);

        // JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        // // Configure Unity Transport
        // var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        // var clientData = AllocationUtils.ToRelayServerData(allocation, "wss");
        // transport.SetRelayServerData(clientData);

        // loadingPanel.SetActive(false);
        // NetworkManager.Singleton.StartClient();
        // }

#endif



    }


    public async void StartSever()
    {
        SetLoadingActive(true, "Creating...");
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3); // max 3 clients

        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Debug.Log("Relay join code: " + joinCode);

        // Configure Unity Transport
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        var serverData = AllocationUtils.ToRelayServerData(allocation, "wss");
        transport.SetRelayServerData(serverData);

        NetworkManager.Singleton.StartServer();
        loadingPanel.SetActive(false);

        // generate qr
        qrDisplay.texture = QRCodeUnity.GenerateQR(URL + joinCode, 256, 256);
        relayCodeText.text = "Code: " + joinCode;

        SetLoadingActive(false);
    }

    public GameObject panel;
    public async void StartClient(string joinCode)
    {

        SetLoadingActive(true, "Joining...");
        if (String.IsNullOrEmpty(joinCode)) return;
        try
        {

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            // Configure Unity Transport
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var clientData = AllocationUtils.ToRelayServerData(allocation, "wss");
            transport.SetRelayServerData(clientData);

            loadingPanel.SetActive(false);
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            panel.SetActive(true);
            Debug.LogError("❌ JoinAllocation failed: " + e.Message);

        }

        SetLoadingActive(false);
    }

    private void SetLoadingActive(bool isActive, string msg = "")
    {
        loadingPanel.SetActive(isActive);
        loadingPanel.GetComponentInChildren<Text>().text = msg;
    }




    //         var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    //         ipInput.onValueChanged.AddListener(Connect);
    // #if UNITY_WEBGL && !UNITY_EDITOR
    //         ipAddress = GetUrlParam("ip"); // e.g. ?ip=127.0.0.1
    //         Debug.Log("IP param from URL: " + ipAddress);
    //         ipText.text = "Local IP Address: " + ipAddress;
    //         transport.ConnectionData.ServerListenAddress = "wss://";
    //         transport.SetConnectionData(ipAddress, port);
    //         return;
    // #endif

    //         ipAddress = GetLocalIPAddress(); // fallback for Editor
    //         Debug.Log("Editor fallback IP: " + ipAddress);
    //         ipText.text = "Local IP Address: " + ipAddress;
    //         transport.ConnectionData.ServerListenAddress = "wss://";
    //         transport.ConnectionData.Address = ipAddress;
    //         transport.ConnectionData.Port = port;
    //         transport.SetConnectionData(ipAddress, port);

    // IEnumerator GetIP()
    // {
    //     UnityWebRequest request = UnityWebRequest.Get("https://api.ipify.org"); // returns your public IP
    //     yield return request.SendWebRequest();

    //     if (request.result == UnityWebRequest.Result.Success)
    //     {
    //         string publicIP = request.downloadHandler.text;
    //         Debug.Log("Public IP: " + publicIP);
    //     }
    //     else
    //     {
    //         Debug.Log("Error: " + request.error);
    //     }
    // }

}



/* * * * *
 * URLParameters.cs
 * ----------------
 * 
 * This singleton script provides easy access to any URL components in a Web-build
 * Just use
 * 
 *     URLParameters.Instance.RegisterOnDone(OnDone);
 *     
 * To register a callback which will be invoked from javascript. The callback receives a
 * reference to the singleton instance. The instance has several properties to hold all the
 * different parts of the URL. In addition it will split and parse the search and hash /
 * fragment value into key/value pairs stored in a dictionary (SearchParameters, HashParameters)
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2012-2017 Markus Göbel (Bunny83)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * * * * */



public class URLParameters : MonoBehaviour
{
    // set testíng data here for in-editor-use
    // href | hash | host | hostname | pathname | port | protocol | search
    public static string TestData = "|||||||";
    private static URLParameters m_Instance = null;

    public static URLParameters Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = (URLParameters)FindObjectOfType(typeof(URLParameters));
                if (m_Instance == null)
                    m_Instance = (new GameObject("URLParameters")).AddComponent<URLParameters>();
                m_Instance.gameObject.name = "URLParameters";
                DontDestroyOnLoad(m_Instance.gameObject);
            }
            return m_Instance;
        }
    }

    private System.Action<URLParameters> m_OnDone = null;
    private System.Action<URLParameters> m_OnDoneOnce = null;

    private bool m_HaveInformation = false;
    private string m_RawData;
    private string m_Href;
    private string m_Hash;
    private string m_Host;
    private string m_Hostname;
    private string m_Pathname;
    private string m_Port;
    private string m_Protocol;
    private string m_Search;
    private Dictionary<string, string> m_SearchParams = new Dictionary<string, string>();
    private Dictionary<string, string> m_HashParams = new Dictionary<string, string>();

    public bool HaveInformation
    {
        get { return m_HaveInformation; }
    }
    public string RawData
    {
        get { return m_RawData; }
    }
    public string Href
    {
        get { return m_Href; }
    }
    public string Hash
    {
        get { return m_Hash; }
    }
    public string Host
    {
        get { return m_Host; }
    }
    public string Hostname
    {
        get { return m_Hostname; }
    }
    public string Pathname
    {
        get { return m_Pathname; }
    }
    public string Port
    {
        get { return m_Port; }
    }
    public string Protocol
    {
        get { return m_Protocol; }
    }
    public string Search
    {
        get { return m_Search; }
    }
    public IDictionary<string, string> SearchParameters
    {
        get { return m_SearchParams; }
    }
    public IDictionary<string, string> HashParameters
    {
        get { return m_HashParams; }
    }


    public void RegisterOnDone(System.Action<URLParameters> aCallback)
    {
        m_OnDone += aCallback;
        if (HaveInformation)
            aCallback(this);
    }

    public void RegisterOnceOnDone(System.Action<URLParameters> aCallback)
    {
        if (HaveInformation)
            aCallback(this);
        else
            m_OnDoneOnce += aCallback;
    }


    public void Request()
    {
        StartCoroutine(_Request());
    }

    private IEnumerator _Request()
    {
        m_HaveInformation = false;
#if UNITY_EDITOR
        yield return null;
        SetAddressComponents(TestData);
#elif UNITY_WEBPLAYER
        const string WebplayerCode = "GetUnity ().SendMessage ('{0}', 'SetAddressComponents', location.href +'|'+ location.hash +'|'+ location.host +'|'+ location.hostname +'|'+ location.pathname +'|'+ location.port +'|'+ location.protocol +'|'+ location.search);";
        Application.ExternalEval(string.Format(WebplayerCode, gameObject.name));
#elif UNITY_WEBGL
        const string WebGLCode = "SendMessage ('{0}', 'SetAddressComponents', location.href +'|'+ location.hash +'|'+ location.host +'|'+ location.hostname +'|'+ location.pathname +'|'+ location.port +'|'+ location.protocol +'|'+ location.search);";
        Application.ExternalEval(string.Format(WebGLCode, gameObject.name));
#endif
        yield break;
    }

    public IEnumerator Start()
    {
        yield return null; // wait one frame to ensure all delegates are assigned.
        Request();
    }

    public void SetAddressComponents(string aData)
    {
        string[] parts = aData.Split('|');
        m_RawData = aData;
        m_Href = parts[0];
        m_Hash = parts[1];
        m_Host = parts[2];
        m_Hostname = parts[3];
        m_Pathname = parts[4];
        m_Port = parts[5];
        m_Protocol = parts[6];
        m_Search = parts[7];
        var tmp = m_Search.TrimStart('?');
        var data = tmp.Split('&');
        for (int i = 0; i < data.Length; i++)
        {
            var val = data[i].Split('=');
            if (val.Length != 2)
                continue;
            m_SearchParams[val[0]] = val[1];
        }
        tmp = m_Hash.TrimStart('#');
        data = tmp.Split('&');
        for (int i = 0; i < data.Length; i++)
        {
            var val = data[i].Split('=');
            if (val.Length != 2)
                continue;
            m_HashParams[val[0]] = val[1];
        }

        m_HaveInformation = true;
        if (m_OnDone != null)
            m_OnDone(this);
        if (m_OnDoneOnce != null)
        {
            m_OnDoneOnce(this);
            m_OnDoneOnce = null;
        }
    }
}


public static class IDictionaryExtension
{
    public static double GetDouble(this IDictionary<string, string> aDict, string aKey, double aDefault = 0d)
    {
        string tmp;
        if (aDict.TryGetValue(aKey, out tmp))
        {
            double val;
            if (double.TryParse(tmp, out val))
                return val;
        }
        return aDefault;
    }
    public static int GetInt(this IDictionary<string, string> aDict, string aKey, int aDefault = 0)
    {
        string tmp;
        if (aDict.TryGetValue(aKey, out tmp))
        {
            int val;
            if (int.TryParse(tmp, out val))
                return val;
        }
        return aDefault;
    }
}