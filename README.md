# 9 Tiles

提出作品の公開リポジトリです。

## 概要
- 作品名　：9 Tiles
- 著作者　：冨士原健斗
- 開発環境：Windows10 + Unity2021.1.24f + Visual Studio 2022
- 動作環境：WebGL
- 開発期間：2022年2月～2022年3月（1ヶ月半）
- 開発人数：1人

スタート地点からゴール地点までキャラクターを移動させるゲームです。

## 操作とルール
- マウスで操作
- ステージセレクトからステージを選べます
- 画面左側のボタンで「チップ」を選び、右側のタイルに配置することでプログラムを作れます
- 「プログラム実行」ボタンを押すとチップが実行されていきます
- 緑色のスタート地点から赤色のゴール地点まで移動できればステージクリアです

## 公開先
- [ゲームのページを開く](https://unityroom.com/games/ninetiles)

## プロジェクトをUnityで開くときの手順
- Assets/9Tiles シーンを開く
- 標準解像度は1280x720ピクセル
- 2Dビュー

# スクリプト概要
本プロジェクト用に開発したスクリプトは以下の通りです。

- Assets/Maze/Scripts
  - Grobal.cs
    - 様々なスクリプトで頻繁に使用される定数や関数を定義
  - MapEditor.cs
    - マップエディット機能
  - MapInformation.cs
    - マップデータの保持・セーブ＆ロードなどを管理
  - ProgramEditor.cs
    - ゲーム内プログラムの編集機能
  - ProgramExecutor.cs
    - ゲーム内プログラムの実行機能
  - TileSelector.cs
    - マップやプログラムを描画するTilemapを管理する
  - Controllers/MainCameraController.cs
    - カメラの移動・拡縮操作
  - Controllers/PlayerController.cs
    - プレイヤーキャラクターの移動などを管理
  - Managers/PlayerManualController.cs
    - プレイヤーキャラクターの手動操作機能
  - Managers/ModeManager.cs
    - タイトル、ステージセレクトなどのモード遷移を管理
  - Managers/SaveLoadManager.cs
    - ステージのクリア記録を管理
  - Managers/SoundManager.cs
    - 効果音・BGMの再生
  - Managers/StageSelectManager.cs
    - ステージセレクト画面のオブジェクト管理


# 組み込みアセットのライセンス
- 本作品ではフォントとして「Noto Sans JP」を使用しています。
  - https://fonts.google.com/noto/specimen/Noto+Sans
  - Licensed under the Open Font License.(https://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL_web)

# 更新履歴
- Ver1.0 最初の公開
