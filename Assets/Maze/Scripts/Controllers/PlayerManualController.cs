using UnityEngine;


/// <summary>
/// プレイヤーキャラクターの手動操作を提供するクラス
/// </summary>
public class PlayerManualController : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController;

    /// <summary>
    /// プレイヤーオブジェクトを検索して保持
    /// </summary>
    void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    /// <summary>
    /// キー操作に応じた処理を呼び出す
    /// </summary>
    void Update()
    {	
    	//左へ回転
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
			playerController.Rotate(-1);
        } 
    	//右へ回転
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            playerController.Rotate(1);
        } 
    	//前進
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            playerController.Move(0);
        } 
    	//後退
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            playerController.Move(2);
        }
    	
    }
}
