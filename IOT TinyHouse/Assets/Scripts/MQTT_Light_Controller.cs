using System;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;

public class MqttLightController : M2MqttUnityClient
{
    public LightSwitchController frontSwitchController;
    public LightSwitchController kitchenSwitchController;
    public LightSwitchController bathroomSwitchController;
    public LightSwitchController bedroomSwitchController;

    private const string MQTT_TOPIC_SUB = "sandbox/fromMiddleHouse";
    private const string MQTT_TOPIC_PUB = "sandbox/toMiddleHouse";

    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] { MQTT_TOPIC_SUB }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        Debug.Log("[MQTT] Subscribed to topic: " + MQTT_TOPIC_SUB);
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        string msg = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log($"[MQTT] Received message: {msg}");

        if (string.IsNullOrWhiteSpace(msg))
        {
            Debug.LogError("[MQTT] Received an empty message!");
            return;
        }

        ProcessMessage(msg);
    }

    private void ProcessMessage(string msg)
    {
        try
        {
            bool isLedComponent = msg.Contains("\"component\":\"led\"");
            if (!isLedComponent)
            {
                Debug.Log($"[MQTT] Message ignored (not a LED component): {msg}");
                return;
            }

            LightSwitchController targetRoom = null;

            if (msg.Contains("\"room\":\"front\""))
            {
                targetRoom = frontSwitchController;
                Debug.Log("[MQTT] Identified 'front' room.");
            }
            else if (msg.Contains("\"room\":\"living\""))
            {
                targetRoom = kitchenSwitchController;
                Debug.Log("[MQTT] Identified 'living' room.");
            }
            else if (msg.Contains("\"room\":\"bathroom\""))
            {
                targetRoom = bathroomSwitchController;
                Debug.Log("[MQTT] Identified 'bathroom' room.");
            }
            else if (msg.Contains("\"room\":\"bedroom\""))
            {
                targetRoom = bedroomSwitchController;
                Debug.Log("[MQTT] Identified 'bedroom' room.");
            }
            else
            {
                Debug.LogWarning("[MQTT] Unknown room, ignoring message.");
                return;
            }

            if (targetRoom == null)
            {
                Debug.LogError("[MQTT] LightSwitchController for the room is not assigned!");
                return;
            }

            bool turnOn = msg.Contains("\"value\":1");
            bool turnOff = msg.Contains("\"value\":0");

            if (!turnOn && !turnOff)
            {
                Debug.LogWarning("[MQTT] Message doesn't contain a valid 'value' field (1 or 0), ignoring.");
                return;
            }

            Debug.Log($"[MQTT] Toggling light for {targetRoom.name}. Turn On: {turnOn}");

            if ((turnOn && !targetRoom.isLightOn) || (turnOff && targetRoom.isLightOn))
            {
                targetRoom.ToggleLights();
                Debug.Log($"[MQTT] Light toggled successfully for {targetRoom.name}.");
            }
            else
            {
                Debug.Log($"[MQTT] Light state unchanged for {targetRoom.name} (already in correct state).");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[MQTT] Error processing MQTT message: " + e.Message);
        }
    }

    public void PublishLightState(string house, string room, bool value)
    {
        string jsonMessage = $"{{\"house\":\"{house}\",\"room\":\"{room}\",\"component\":\"led\",\"value\":{(value ? 1 : 0)},\"msg\":\"LED {(value ? "on" : "off")}\"}}";
        client.Publish(MQTT_TOPIC_PUB, System.Text.Encoding.UTF8.GetBytes(jsonMessage), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        Debug.Log($"[MQTT] Published message: {jsonMessage}");
    }
}
