using System.Collections; // 「コルーチン（時間を操る処理）」を使うための準備
using Unity.Netcode;     
using UnityEngine;     

// NetworkBehaviourを継承して、ネットワーク越しに同期・通信できるオブジェクトにする
public class NetworkGrabbable : NetworkBehaviour
{
    // 物理演算（重力や衝突）を管理するコンポーネントを入れる箱
    private Rigidbody rb;

    // --- 【初期化処理：ゲームが始まった瞬間に呼ばれる】 ---
    void Awake()
    {
        // 自分のオブジェクトについている Rigidbody（物理エンジン）を探して箱にしまう
        rb = GetComponent<Rigidbody>();
    }

    // --- 【掴んだ瞬間に呼ばれる処理（自分の画面での処理）】 ---
    // MetaのSDKの「掴んだよ！」というイベント（Event Wrapper）から直接呼び出される
    public void OnGrab()
    {
        // 【ローカル先行処理（ラグ対策）】
        // サーバーからの返事を待たずに、自分の画面だけ先に物理演算（重力）をOFFにする！
        rb.isKinematic = true;

        // 裏でサーバーに向かって「自分が掴んだから、権限ちょうだい！」とお願いの電話（RPC）をかける
        // NetworkManager.Singleton.LocalClientId には「自分のプレイヤーID」が入っている
        GrabServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    // --- 【離した瞬間に呼ばれる処理（自分の画面での処理）】 ---
    public void OnRelease()
    {
        // 直接OFFにするとMetaのSDKとケンカする（上書き負けする）ので、
        // コルーチンという「時間差で実行する機能」をスタートさせる
        StartCoroutine(ReleaseRoutine());
    }

    // --- 【コルーチン：1フレーム待ってから物理を再開するおまじない】 ---
    private IEnumerator ReleaseRoutine()
    {
        // yield return null は「このフレームの処理はいったん終わり！次のフレームまで待つ！」という命令
        yield return null;

        // 1フレーム待ったことでMeta SDKの処理が完全に終わったので、
        // 安全に後出しジャンケンで物理演算（重力）をON（落下開始）に戻す！
        rb.isKinematic = false;

        // サーバーに向かって「手放したよ！」と報告の電話（RPC）をかける
        ReleaseServerRpc();
    }

    // --- 【サーバー側の処理①：掴まれた時】 ---
    // [ServerRpc] は「クライアント（子）から呼ばれるけど、実際に動くのはサーバー（親）だけ」という特殊な関数
    // RequireOwnership = false は「まだ自分の所有物じゃなくても、この電話をかけていいよ」という許可設定
    [ServerRpc(RequireOwnership = false)]
    private void GrabServerRpc(ulong newOwnerId)
    {
        // ネットワークオブジェクトの「所有権（Ownership）」を、掴んだ人（newOwnerId）に変更する
        // これにより、以降の動きはその人がホストとなって全員の画面に同期されるようになる
        GetComponent<NetworkObject>().ChangeOwnership(newOwnerId);

        // サーバー側（＝全員の共通ルール）でも、このアイテムの重力をOFFにする
        rb.isKinematic = true;
    }

    // --- 【サーバー側の処理②：離された時】 ---
    [ServerRpc(RequireOwnership = false)]
    private void ReleaseServerRpc()
    {
        // サーバー側（＝全員の共通ルール）でも、このアイテムの重力をON（落下開始）にする
        rb.isKinematic = false;
    }
}