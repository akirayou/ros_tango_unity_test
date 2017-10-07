using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ros_CSharp;
using tf.net;
using GM = Messages.geometry_msgs;






public class RosManager : MonoBehaviour {
    public Transform TangoCamera;
    int FrameSkip = 4; //Broadcasting TF slow down factor
    //set ROS HOST It's usefule for multi NIC environmet(such as have ROS network and Internet)
    /// <summary>
    /// set Network configration (ROS_MASTER_URI ,ROS_IP  and so on)
    /// </summary>
    void setROS_HOST()
    {
        string hostname = System.Net.Dns.GetHostName();
        //ROS.ROS_HOSTNAME = hostname; //In many case network for ROS have invalid name... I Dont't use this.
        System.Net.IPAddress[] adrList = System.Net.Dns.GetHostAddresses(hostname);
        foreach (System.Net.IPAddress address in adrList)
        {
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                address.ToString().Substring(0, 11) == "192.168.56.") //Have VirtualBox Ubuntu
            {  // to select NIC for ROS. This sample means 10.0.0.0/8
                ROS.ROS_IP = address.ToString();
                ROS.ROS_MASTER_URI = "http://192.168.56.101:11311";
                break;//Priority connectin have break
            }
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                address.ToString().Substring(0, 10) == "192.168.1.") //Have VirtualBox Ubuntu
            {  // to select NIC for ROS. This sample means 10.0.0.0/8
                ROS.ROS_IP = address.ToString();
                ROS.ROS_MASTER_URI = "http://192.168.1.212:11311";
                //Not priority connection dose not have break
            }
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                address.ToString().Substring(0, 3) == "10.")
            {  // to select NIC for ROS. This sample means 10.0.0.0/8
                ROS.ROS_IP = address.ToString();
                ROS.ROS_MASTER_URI = "http://10.0.1.101:11311";
            }
        }
    }


    /// <summary>
    /// Poll coordinate data and push to TF data (User Logic is here)
    /// </summary>
    void Poll()
    {
        GM.TransformStamped tfdata = BlankTf();
        tfdata.header.frame_id = "tango_base";
        tfdata.child_frame_id = "tango_device";
        tfdata.transform.translation.x = TangoCamera.position.x;
        tfdata.transform.translation.y = TangoCamera.position.y;
        tfdata.transform.translation.z = TangoCamera.position.z;
        tfdata.transform.rotation.x = TangoCamera.rotation.x;
        tfdata.transform.rotation.y = TangoCamera.rotation.y;
        tfdata.transform.rotation.z = TangoCamera.rotation.z;
        tfdata.transform.rotation.w = TangoCamera.rotation.w;
        AddTf(tfdata);

        //FYI
        //to deepcopy tf data,use fllowing.
        //tfdata_to.Deserialize(tfdata_from.Serialize());

    }











    //ROS Rlated member
    NodeHandle n;
    Transformer tf;
    Publisher<Messages.tf.tfMessage> tf_pub;
    List<GM.TransformStamped> BroadcastList = new List<GM.TransformStamped>();

    RosManager()
    {
    }
    void Awake()
    {
     
    }
    // Use this for initialization
    void Start () {
        setROS_HOST();
        string[] dummy = { "" };
        ROS.Init(dummy, "unity");//TODO:set your own node name
        n = new NodeHandle();
        tf = new Transformer(false/* do Interprate ?*/); /* for TF lookup*/
        tf_pub = n.advertise<Messages.tf.tfMessage>("/tf", 100); /* for TF broad cast */

        Debug.LogError("ROS my HostIP  :" + ROS.ROS_IP + "\nMASTER_URI" + ROS.ROS_MASTER_URI);
    }
    /// <summary>
    /// get blank TransformStamped all member is filled (no null pointer)
    /// you can just fill members such as x,y,z 
    /// </summary>
    /// <returns>Blank TransformStamped data for addTf</returns>
    public GM.TransformStamped BlankTf()
    {
        GM.TransformStamped tfdata = new GM.TransformStamped();
        tfdata.header = new Messages.std_msgs.Header();
        tfdata.transform = new GM.Transform();
        tfdata.header.stamp = ROS.GetTime();
        tfdata.transform = new GM.Transform();
        tfdata.transform.translation = new GM.Vector3();
        tfdata.transform.rotation = new GM.Quaternion();
        return tfdata;
    }


    uint tfSeq = 0;
    /// <summary>
    /// add TF data, they will publish next Update().
    /// </summary>
    /// <param name="tfdata"></param>
    public void AddTf(GM.TransformStamped tfdata)
    {
        lock (BroadcastList)
        {

            
            tfdata.header.seq = tfSeq;
            BroadcastList.Add(tfdata);
            tfSeq++;

        }
    }

    int pollCount = 0;
    // Update is called once per frame
    void Update() {
        pollCount++;
        if (pollCount > FrameSkip)
        {
            pollCount = 0;

            Poll();
            lock (BroadcastList)
            {
                if (BroadcastList.Count != 0)
                {
                    Messages.tf.tfMessage tfm = new Messages.tf.tfMessage();
                    tfm.transforms = BroadcastList.ToArray();
                    tf_pub.publish(tfm);
                    BroadcastList.Clear();
                }
            }
        }
     }
}
