using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ros_CSharp;
using tf.net;
using gm = Messages.geometry_msgs;






public class RosManager : MonoBehaviour {
    NodeHandle n;
    Transformer tf;
    Publisher<Messages.tf.tfMessage> tf_pub;

    List<gm.TransformStamped> updateList=new List<gm.TransformStamped>();


    //set ROS HOST It's usefule for multi NIC environmet(such as have ROS network and Internet)
    private void setROS_HOST()
    {
        string hostname = System.Net.Dns.GetHostName();
        //ROS.ROS_HOSTNAME = hostname; //In many case network for ROS have invalid name... I Dont't use this.
        System.Net.IPAddress[] adrList = System.Net.Dns.GetHostAddresses(hostname);
        foreach (System.Net.IPAddress address in adrList)
        {
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                address.ToString().Substring(0, 3) == "10.") {  //TODO: to select NIC for ROS. This sample means 10.0.0.0/8
                ROS.ROS_IP = address.ToString();
            }
        }
        //Debug.Log("ROS my HostName:" + ROS.ROS_HOSTNAME);
        Debug.Log("ROS my HostIP  :" + ROS.ROS_IP);
    }

    RosManager()
    {

    }    
    void Awake()
    {
        setROS_HOST();
        ROS.ROS_MASTER_URI = "http://10.0.1.101:11311";
        string[] dummy = { "" };
        ROS.Init(dummy, "unity");
        n = new NodeHandle();
        tf = new Transformer(false/* do Interprate ?*/);
        tf_pub = n.advertise<Messages.tf.tfMessage>("/tf", 100);
    }

    // Use this for initialization
    void Start () {

    }


    // Update is called once per frame
    void Update () {





        gm.TransformStamped tfdata = new gm.TransformStamped();
        tfdata.header = new Messages.std_msgs.Header();
        tfdata.transform = new gm.Transform();
        tfdata.header.stamp = ROS.GetTime();
        tfdata.transform = new gm.Transform();
        tfdata.transform.translation = new gm.Vector3();
        tfdata.transform.rotation = new gm.Quaternion();


        tfdata.header.frame_id = "tango_base";
        tfdata.child_frame_id = "tango_device";
        tfdata.transform.translation.x = 0;
        tfdata.transform.translation.y = 1;
        tfdata.transform.translation.z = 2;
        tfdata.transform.rotation.w = 1;
        tfdata.transform.rotation.x = 0;
        tfdata.transform.rotation.y = 0;
        tfdata.transform.rotation.z = 0;
        



        Messages.tf.tfMessage tfm = new Messages.tf.tfMessage();
        tfm.transforms = new gm.TransformStamped[] { tfdata };

        tf_pub.publish(tfm);



        }

    }
}
