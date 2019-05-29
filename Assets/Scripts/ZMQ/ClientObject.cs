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
    public LogfileReader logfileReader;

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

        if (message.Length > 10)
        {
            message = message.Replace(" ", "");
            message = message.Split('{')[1].Split('}')[0];
            string[] elements = message.Split(',');

            int x = -1, y = -1, z = -1, yaw = -1, m1 = -1, m2 = -1, m3 = -1, m4 = -1, m5 = -1;
            int setAmount = 0;

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i].Split(':')[0] == "\"x\"")
                {
                    x = (int)System.Convert.ToSingle(elements[i].Split(':')[1].Replace(",", ","));
                    setAmount++;
                }
                else if (elements[i].Split(':')[0] == "\"y\"")
                {
                    y = (int)System.Convert.ToSingle(elements[i].Split(':')[1].Replace(",", ","));
                    setAmount++;
                }
                else if(elements[i].Split(':')[0] == "\"z\"")
                {
                    z = (int)System.Convert.ToSingle(elements[i].Split(':')[1].Replace(",", ","));
                    setAmount++;
                }
                else if (elements[i].Split(':')[0] == "\"yaw\"")
                {
                    yaw = (int)System.Convert.ToSingle(elements[i].Split(':')[1].Replace(",", ","));
                    setAmount++;
                }
                else if (elements[i].Split(':')[0] == "\"distance_front\"")
                {
                    m1 = (int)System.Convert.ToSingle(elements[i].Split(':')[1].Replace(",", ","));
                    setAmount++;
                }
                else if (elements[i].Split(':')[0] == "\"distance_back\"")
                {
                    m2 = (int)System.Convert.ToSingle(elements[i].Split(':')[1].Replace(",", ","));
                    setAmount++;
                }
                else if (elements[i].Split(':')[0] == "\"distance_up\"")
                {
                    m3 = (int)System.Convert.ToSingle(elements[i].Split(':')[1].Replace(",", ","));
                    setAmount++;
                }
                else if(elements[i].Split(':')[0] == "\"distance_left\"")
                {
                    m4 = (int)System.Convert.ToSingle(elements[i].Split(':')[1].Replace(",", ","));
                    setAmount++;
                }
                else if (elements[i].Split(':')[0] == "\"distance_right\"")
                {
                    m5 = (int)System.Convert.ToSingle(elements[i].Split(':')[1].Replace(",", ","));
                    setAmount++;
                }
            }

            if (setAmount == 9)
            {
                messagesText.text += "Added Dataset (" + x.ToString() + "," + y.ToString() + "," + z.ToString() + "," + yaw.ToString() + "," + m1.ToString() + ") from ("
                    + message + ") #";
                logfileReader.AddDataSetToDataholder(x, y, z, yaw, m1, m2, m3, m4, m5);
            }
            else
            {
                messagesText.text += "Not added Dataset (" + x.ToString() + "," + y.ToString() + ","  + z.ToString() + "," + yaw.ToString() + "," + m1.ToString() + ")#";
            }
        }


        //messagesText.text += "\n" + "[m] " + message;

        //System.IO.File.AppendAllText("./zmqmessages.log", "\n" + message);
    }

    public void ClearText()
    {
        messagesText.text = "";

        //HandleMessage("{\"x\": -0.5528, \"y\": -0.7506, \"z\": 500.4082, \"distance_front\": 328, \"distance_back\": 1432, \"distance_up\": 3545, \"distance_left\": 1385, \"distance_right\": 660, \"yaw\": -3.4013359546661377}");
        //HandleMessage("pose\n{\"x\": -1.5528, \"y\": -1.7506, \"z\": 501.4082, \"distance_front\": 328, \"distance_back\": 1432, \"distance_up\": 3545, \"distance_left\": 1385, \"distance_right\": 660, \"yaw\": -3.4013359546661377}");
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
        inputIP.text = "tcp://127.0.0.1:5578";
        inputTopic.text = "pose";
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
