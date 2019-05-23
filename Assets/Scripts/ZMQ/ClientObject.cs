using System.Collections.Concurrent;
using System.Threading;
using NetMQ;
using UnityEngine;
using NetMQ.Sockets;
using UnityEngine.UI;

public class NetMqListener
{
    private string _serverIP;
    private string _topic;

    private readonly Thread _listenerWorker;

    private bool _listenerCancelled;

    public delegate void MessageDelegate(string message);

    private readonly MessageDelegate _messageDelegate;

    private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        using (var subSocket = new SubscriberSocket())
        {
            subSocket.Options.ReceiveHighWatermark = 1000;
            subSocket.Connect(_serverIP);
            //subSocket.Connect("tcp://localhost:12345");
            subSocket.Subscribe(_topic);
            while (!_listenerCancelled)
            {
                string frameString;
                if (!subSocket.TryReceiveFrameString(out frameString)) continue;
                Debug.Log(frameString);
                _messageQueue.Enqueue(frameString);
            }
            subSocket.Close();
        }
        NetMQConfig.Cleanup();
    }

    public void Update()
    {
        while (!_messageQueue.IsEmpty)
        {
            string message;
            if (_messageQueue.TryDequeue(out message))
            {
                _messageDelegate(message);
            }
            else
            {
                break;
            }
        }
    }

    public NetMqListener(MessageDelegate messageDelegate, string serverIP, string topic)
    {
        _serverIP = serverIP;
        _topic = topic;
        _messageDelegate = messageDelegate;
        _listenerWorker = new Thread(ListenerWork);
    }

    public void Start()
    {
        _listenerCancelled = false;
        _listenerWorker.Start();
    }

    public void Stop()
    {
        _listenerCancelled = true;
        _listenerWorker.Join();
    }
}

public class ClientObject : MonoBehaviour
{
    private NetMqListener _netMqListener = null;

    public Text messagesText;
    public InputField inputIP;
    public InputField inputTopic;

    private void HandleMessage(string message)
    {
        /*var splittedStrings = message.Split(' ');
        if (splittedStrings.Length != 3) return;
        var x = float.Parse(splittedStrings[0]);
        var y = float.Parse(splittedStrings[1]);
        var z = float.Parse(splittedStrings[2]);
        transform.position = new Vector3(x, y, z);*/

        messagesText.text += "\n" + "[m] " + message;

        System.IO.File.AppendAllText("./zmqmessages.log", "\n" + message);
    }

    public void ClearText()
    {
        messagesText.text = "";
    }
    
    public void ConnectToZMQ()
    {
        DisconnectZMQ();

        string server = inputIP.text;//"tcp://localhost:12345";
        string topic = inputTopic.text;
        messagesText.text = "Starting client for " + server + " listening for topic " + topic;
        _netMqListener = new NetMqListener(HandleMessage, server, topic);
        _netMqListener.Start();
    }

    public void DisconnectZMQ()
    {
        if (_netMqListener != null)
        {
            Debug.Log("Stopping thread");
            _netMqListener.Stop();
        }
    }

    private void Start()
    {
        if (System.IO.File.Exists("./zmqmessages.log") == false)
        {
            System.IO.File.WriteAllText("./zmqmessages.log", "");
        }
        System.IO.File.AppendAllText("./zmqmessages.log", "\n######### New Run #############\n");
        //string server = "tcp://127.0.0.1";
    }

    private void Update()
    {
        if (_netMqListener != null)
        {
            _netMqListener.Update();
        }
    }

    private void OnDestroy()
    {
        _netMqListener.Stop();
    }
}
