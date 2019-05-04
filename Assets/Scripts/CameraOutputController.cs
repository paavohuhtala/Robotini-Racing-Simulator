﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class CameraOutputController : MonoBehaviour
{
    public Camera camera;
    private float lastSaved = 0;

    // Start is called before the first frame update
    void Start()
    {
        AsynchronousSocketListener.StartListening();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time < lastSaved + 0.03)
        {
            return;
        }

        lastSaved = Time.time;
        int width = 128;
        int height = 80;

        camera.aspect = 1.0f;
        // recall that the height is now the "actual" size from now on

        RenderTexture tempRT = new RenderTexture(width, height, 24);
        // the 24 can be 0,16,24, formats like
        // RenderTextureFormat.Default, ARGB32 etc.

        camera.targetTexture = tempRT;
        camera.Render();

        RenderTexture.active = tempRT;
        Texture2D virtualPhoto =
            new Texture2D(width, height, TextureFormat.RGB24, false);
        // false, meaning no need for mipmaps
        virtualPhoto.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        RenderTexture.active = null; //can help avoid errors 
        camera.targetTexture = null;
        // consider ... Destroy(tempRT);

        byte[] bytes;
        bytes = virtualPhoto.EncodeToPNG();

        AsynchronousSocketListener.SendFrame(bytes);
        // virtualCam.SetActive(false); ... no great need for this.
    }
}

// State object for reading client data asynchronously  
public class StateObject
{
    // Client  socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 1024;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
}

public class AsynchronousSocketListener
{
    // Thread signal.  
    public static ManualResetEvent allDone = new ManualResetEvent(false);
    static bool listening = false;
    static StateObject globalState = null;
    static bool sending = false;

    public AsynchronousSocketListener()
    {
    }

    public static void SendFrame(byte[] data)
    {
        if (globalState != null && !sending)
        {
            sending = true;

            string base64Encoded = System.Convert.ToBase64String(data) + "\n";

            byte[] encodedBytes = System.Text.Encoding.ASCII.GetBytes(base64Encoded);


            Socket socket = globalState.workSocket;
            Debug.Log("Sending data to TCP socket");
            // Begin sending the data to the remote device.
            AsyncCallback callback = new AsyncCallback((IAsyncResult ar) => {
                try
                {
                    int bytesSent = socket.EndSend(ar);
                    Debug.Log("Sent " + encodedBytes.Length + " bytes to client.");
                }
                catch (Exception e)
                {
                    Debug.Log("Socket send failed:" + e.ToString());
                    globalState = null;
                }
                sending = false;
            });

            socket.BeginSend(encodedBytes, 0, encodedBytes.Length, 0, callback, socket);
        }
    }

    public static void StartListening()
    {
        if (listening) return;
        listening = true;
        // Establish the local endpoint for the socket.  
        // The DNS name of the computer  
        // running the listener is "host.contoso.com".  
        IPAddress ipAddress = IPAddress.Any;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        // Create a TCP/IP socket.  
        Socket listener = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        listener.Bind(localEndPoint);
        listener.Listen(100);

        Debug.Log("Starting thread");

        new Thread(() =>
        {
            while (true)
            {
                // Set the event to nonsignaled state.  
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.  
                Debug.Log("Waiting for a connection...");
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    listener);

                // Wait until a connection is made before continuing.  
                allDone.WaitOne();
            }
        }).Start();

    }

    public static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.  
        allDone.Set();

        // Get the socket that handles the client request.  
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        // Create the state object.  
        globalState = new StateObject();
        globalState.workSocket = handler;

        Debug.Log("Client connected");

    }

    public static void ReadCallback(IAsyncResult ar)
    {

    }
}