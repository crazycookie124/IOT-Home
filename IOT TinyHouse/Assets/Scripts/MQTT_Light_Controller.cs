using System;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;

public class MqttLightController : M2MqttUnityClient
{
    // Add references to the LightSwitchController for each room
    public LightSwitchController frontRoomSwitchController;
    public LightSwitchController kitchenSwitchController;
    public LightSwitchController bathroomSwitchController;
    public LightSwitchController bedroomSwitchController;

    private const string MQTT_TOPIC_SUB = "sandbox/fromMiddleHouse";

    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] { MQTT_TOPIC_SUB }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        string msg = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log("Received: " + msg);
        ProcessMessage(msg);
    }

    private void ProcessMessage(string msg)
    {
        try
        {
            // Ensure the message contains the expected keys (room, component, value)
            if (msg.Contains("\"component\":\"led\""))
            {
                if (msg.Contains("\"room\":\"front\""))
                {
                    // Trigger light controller for front room
                    ToggleRoomLight(frontRoomSwitchController, msg);
                }
                else if (msg.Contains("\"room\":\"living\""))
                {
                    // Trigger light controller for kitchen and living room
                    ToggleRoomLight(kitchenSwitchController, msg);
                }
                else if (msg.Contains("\"room\":\"bathroom\""))
                {
                    // Trigger light controller for bathroom (and fan if necessary)
                    ToggleRoomLight(bathroomSwitchController, msg);
                }
                else if (msg.Contains("\"room\":\"bedroom\""))
                {
                    // Trigger light controller for bedroom
                    ToggleRoomLight(bedroomSwitchController, msg);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error processing MQTT message: " + e.Message);
        }
    }

    private void ToggleRoomLight(LightSwitchController roomSwitchController, string msg)
    {
        if (msg.Contains("\"value\":1"))
        {
            // If the message indicates the light should be ON, trigger the light switch controller
            roomSwitchController.ToggleLights();
        }
        else if (msg.Contains("\"value\":0"))
        {
            // If the message indicates the light should be OFF, trigger the light switch controller
            roomSwitchController.ToggleLights();
        }
    }
}