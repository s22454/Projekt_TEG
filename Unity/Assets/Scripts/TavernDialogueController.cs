using UnityEngine;
using System.Collections;

public class TavernDialogueController : MonoBehaviour
{
    private static readonly string _className = "TAVERN DIALOGUE CONTROLLER";

    void Start()
    {
        StartCoroutine(SendEndDayMessage());
    }

    IEnumerator SendEndDayMessage()
    {
        // Send end-of-day message to the pipe
        bool success = PipeSystem.Instance.EncodeAndSendMessageToServer(
            ActionCode.ENDDAY,
            Sender.SYSTEM,
            Item.NULL,
            0, 0, "End of day triggered from TavernDialogueController"
        );

        if (!success)
        {
            LogManager.Log(_className, LogType.ERROR, "Failed to send ENDDAY message through pipe.");
            yield break;
        }

        LogManager.Log(_className, LogType.LOG, "ENDDAY message sent.");

        yield return new WaitForSeconds(5);
        GameManager.Instance.StartNewDay();
    }
}