using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using TMPro;
using System;

public class TCPSocket : MonoBehaviour
{
    private IPAddress m_address;     // 접속할 곳의 IP주소.
    private const int m_port = 7777;     // 접속할 곳의 포트 번호. 
    private Socket m_listener = null; // 리스닝 소켓.
    private Socket m_socket = null; // 통신용 변수.
    private State m_state; // 상태. 
 
    private enum State // 상태정의. 
    {
        SelectHost = 0,
        StartListener,
        AcceptClient,
        ServerCommunication,
        StopListener,
        ClientCommunication,
        Endcommunication,
    }

    // Use this for initialization
    void Start()
    {
        m_state = State.SelectHost;

        IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
        System.Net.IPAddress hostAddress = hostEntry.AddressList[0];
        m_address = IPAddress.Loopback; // 로컬 IPv4

        Debug.Log(hostEntry.HostName);
    }

    // Update is called once per frame
    void Update()
    {
        switch (m_state)
        {
            case State.StartListener:
                StartListener();
                break;

            case State.AcceptClient:
                AcceptClient();
                break;

            case State.ServerCommunication:
                ServerCommunication();
                break;

            case State.StopListener:
                StopListener();
                break;

            case State.ClientCommunication:
                ClientProcess();
                break;

            default:
                break;
        }
    }

    // 대기 시작.
    void StartListener()
    {
        Debug.Log("Start server communication.");

        m_listener = new Socket(AddressFamily.InterNetwork/*IPv4*/, SocketType.Stream, ProtocolType.Tcp); // 소켓을 생성합니다.
        m_listener.Bind(new IPEndPoint(IPAddress.Any, m_port)); // 사용할 포트 번호를 할당합니다
        m_listener.Listen(1); // 대기를 시작합니다. 

        m_state = State.AcceptClient;
    }

    // 클라이언트의 접속 대기.
    void AcceptClient()
    {
        if (m_listener != null && m_listener.Poll(0, SelectMode.SelectRead)) // 수신 못 해서 문제
        {           
            m_socket = m_listener.Accept(); // 클라이언트가 접속했습니다.
            Debug.Log("[TCP]Connected from client.");
            m_state = State.ServerCommunication;
        }
    }
    
    void ServerCommunication() // 클라이언트의 메시지 수신.
    {
        byte[] buffer = new byte[1400];
        int recvSize = m_socket.Receive(buffer, buffer.Length, SocketFlags.None);
        if (recvSize > 0)
        {
            string message = System.Text.Encoding.UTF8.GetString(buffer);
            Debug.Log(message);
            m_state = State.StopListener;
        }
    }

    void StopListener()
    { 
        if (m_listener != null)    // 대기를 종료합니다.
        {
            m_listener.Close();
            m_listener = null;
        }

        m_state = State.Endcommunication;

        Debug.Log("[TCP]End server communication.");
    }

    void ClientProcess() // 클라이언트와의 접속, 송신, 접속 해제.
    {
        Debug.Log("[TCP]Start client communication.");
        Debug.Log(m_address.ToString());

        m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // 서버에 접속.
        m_socket.NoDelay = true;
        m_socket.SendBufferSize = 0;

        // 서버와 연결 시도
        m_socket.Connect(m_address, m_port);
        Debug.Log("[TCP]Connected to the server.");

        byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Hello, this is client."); // 메시지 송신.
        m_socket.Send(buffer, buffer.Length, SocketFlags.None);

        m_socket.Shutdown(SocketShutdown.Both); // 접속 해제. 
        m_socket.Close();

        Debug.Log("[TCP]End client communication.");
    }


    public void LaunchServer() // 버튼
    {
        m_state = State.StartListener;
        Debug.Log("Server is starting...");
    }

    public void ConnectedToServer() // 버튼
    {
        m_state = State.ClientCommunication;
        Debug.Log($"Connecting to server at {m_address}...");
    }
}

