using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using TMPro;
using System;

public class TCPSocket : MonoBehaviour
{
    private IPAddress m_address;     // ������ ���� IP�ּ�.
    private const int m_port = 7777;     // ������ ���� ��Ʈ ��ȣ. 
    private Socket m_listener = null; // ������ ����.
    private Socket m_socket = null; // ��ſ� ����.
    private State m_state; // ����. 
 
    private enum State // ��������. 
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
        m_address = IPAddress.Loopback; // ���� IPv4

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

    // ��� ����.
    void StartListener()
    {
        Debug.Log("Start server communication.");

        m_listener = new Socket(AddressFamily.InterNetwork/*IPv4*/, SocketType.Stream, ProtocolType.Tcp); // ������ �����մϴ�.
        m_listener.Bind(new IPEndPoint(IPAddress.Any, m_port)); // ����� ��Ʈ ��ȣ�� �Ҵ��մϴ�
        m_listener.Listen(1); // ��⸦ �����մϴ�. 

        m_state = State.AcceptClient;
    }

    // Ŭ���̾�Ʈ�� ���� ���.
    void AcceptClient()
    {
        if (m_listener != null && m_listener.Poll(0, SelectMode.SelectRead)) // ���� �� �ؼ� ����
        {           
            m_socket = m_listener.Accept(); // Ŭ���̾�Ʈ�� �����߽��ϴ�.
            Debug.Log("[TCP]Connected from client.");
            m_state = State.ServerCommunication;
        }
    }
    
    void ServerCommunication() // Ŭ���̾�Ʈ�� �޽��� ����.
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
        if (m_listener != null)    // ��⸦ �����մϴ�.
        {
            m_listener.Close();
            m_listener = null;
        }

        m_state = State.Endcommunication;

        Debug.Log("[TCP]End server communication.");
    }

    void ClientProcess() // Ŭ���̾�Ʈ���� ����, �۽�, ���� ����.
    {
        Debug.Log("[TCP]Start client communication.");
        Debug.Log(m_address.ToString());

        m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // ������ ����.
        m_socket.NoDelay = true;
        m_socket.SendBufferSize = 0;

        // ������ ���� �õ�
        m_socket.Connect(m_address, m_port);
        Debug.Log("[TCP]Connected to the server.");

        byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Hello, this is client."); // �޽��� �۽�.
        m_socket.Send(buffer, buffer.Length, SocketFlags.None);

        m_socket.Shutdown(SocketShutdown.Both); // ���� ����. 
        m_socket.Close();

        Debug.Log("[TCP]End client communication.");
    }


    public void LaunchServer() // ��ư
    {
        m_state = State.StartListener;
        Debug.Log("Server is starting...");
    }

    public void ConnectedToServer() // ��ư
    {
        m_state = State.ClientCommunication;
        Debug.Log($"Connecting to server at {m_address}...");
    }
}

