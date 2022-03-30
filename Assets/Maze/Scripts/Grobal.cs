using UnityEngine;
using System;

namespace Grobal
{
    /// <summary>
    /// ゲーム全体でよく使用される関数を提供するクラス
    /// </summary>
    public static class GrobalMethod
    {
        /// <summary>
        /// あるマスから上下左右に1マス移動した場合、座標はどう変化するかを返す
        /// Unity上の座標は左下から右上へ数え上げていくことに注意する
        /// </summary>
        /// <param name="_direction">向き情報</param>
        /// <returns></returns>
        public static int[] DirToRelativePosition(int _direction)
        {
            //(x,y)の順に格納
            int[] relativePosition = new int[2];

            //負の値を与えられた場合は正になるよう補正
            while (_direction < 0)
            {
                _direction += GrobalConst.DirectionLength;
            }


            switch (_direction % GrobalConst.DirectionLength)
            {
                case (int)GrobalConst.DirectionId.Up:
                    relativePosition = new int[] { 0, 1 };
                    break;

                case (int)GrobalConst.DirectionId.Right:
                    relativePosition = new int[] { 1, 0 };
                    break;

                case (int)GrobalConst.DirectionId.Down:
                    relativePosition = new int[] { 0, -1 };
                    break;

                case (int)GrobalConst.DirectionId.Left:
                    relativePosition = new int[] { -1, 0 };
                    break;
            }

            return relativePosition;
        }

        /// <summary>
        /// DirToRelativePosition(int)のオーバーロード
        /// </summary>
        /// <param name="_direction">向き情報</param>
        /// <returns></returns>
        public static int[] DirToRelativePosition(GrobalConst.DirectionId _direction)
        {
            return DirToRelativePosition((int)_direction);
        }




        /// <summary>
        /// 与えられた向き情報を回転した向き情報を返す
        /// enum型で定義された値がint型の値も持つこと、向き情報のIdリストが右回りに並んでいることを利用して求める
        /// </summary>
        /// <param name="_direction">向き情報</param>
        /// <param name="_num">Idの差。右回りなら正、左回りなら負</param>
        /// <returns></returns>
        public static GrobalConst.DirectionId GetRelativeDirection(GrobalConst.DirectionId _direction, int _num)
        {
            //指定するIdを計算
            int relativeDirection = (int)_direction + _num;

            //指定したIdが負の値になった場合は正の値になるよう補正する
            while (relativeDirection < 0)
            {
                relativeDirection += GrobalConst.DirectionLength;
            }

            //指定したIdがIdリストのサイズを超えた場合は剰余を取る
            while (relativeDirection >= GrobalConst.DirectionLength)
            {
                relativeDirection %= GrobalConst.DirectionLength;
            }

            return (GrobalConst.DirectionId)relativeDirection;
        }



        /// <summary>
        /// 向き情報に対応したZ軸まわりのオイラー角を返す
        /// Unity上の角度は左回りで数えることに注意
        /// </summary>
        /// <param name="_direction">向き情報</param>
        /// <returns></returns>
        public static Quaternion DirToRot(GrobalConst.DirectionId _direction)
        {
            //向き情報は360度を4分割していることを利用
            return Quaternion.Euler(0, 0, (int)_direction * (-90));
        }

    }


    /// <summary>
    /// ゲーム全体でよく使用される共通定数を設定するクラス
    /// </summary>
    public static class GrobalConst
    {
        /// <summary>
        /// 各種タイルのIDを設定
        /// タイルごとに処理を分けるため、一意に決めておく
        /// (他のプログラムで、この順番であることを利用して向き等を計算している場合がある)
        /// </summary>
        public enum TileId{
            //マップを構成するタイル
            //MapInformationクラスのmapData（mapTilemap）で扱う

            /// <summary>
            /// プレイヤーキャラが歩ける、マップ上の床
            /// </summary>
            Floor = 0,

            /// <summary>
            /// プレイヤーキャラが進入できない壁
            /// </summary>
            Wall = 1,

            /// <summary>
            /// マップ上のスタート地点
            /// </summary>
            Start = 2,

            /// <summary>
            /// マップ上のゴール地点
            /// </summary>
            Goal = 3,


            //プログラムマップを構成するタイル（ゲーム内ではチップと呼ぶ）
            //ProgramEditorのprogramMap（programTilemap）で扱う

            /// <summary>
            /// 行動チップ：プレイヤーキャラを前進させる
            /// </summary>
            MoveForward = 4,

            /// <summary>
            /// 行動チップ：プレイヤーキャラを後退させる
            /// </summary>
            MoveBackward = 5,

            /// <summary>
            /// 行動チップ：プレイヤーキャラを右に回転させる
            /// </summary>
            RotateRight = 6,

            /// <summary>
            /// 行動チップ：プレイヤーキャラを左に回転させる
            /// </summary>
            RotateLeft = 7,

            /// <summary>
            /// 分岐チップ：プレイヤーキャラの正面に壁があるかどうか
            /// </summary>
            IfFront = 8,

            /// <summary>
            /// 分岐チップ：プレイヤーキャラの右側に壁があるかどうか
            /// </summary>
            IfRight = 9,

            /// <summary>
            /// 分岐チップ：プレイヤーキャラの背後に壁があるかどうか
            /// </summary>
            IfBack = 10,

            /// <summary>
            /// 分岐チップ：プレイヤーキャラの左側に壁があるかどうか
            /// </summary>
            IfLeft = 11,

            /// <summary>
            /// 行動チップ：何もしない
            /// </summary>
            Wait = 12,


            //プログラムマップの背景を構成するタイル
            //ProgramEditorのprogramBaseMap（programBaseTilemap）で扱う

            /// <summary>
            /// プログラムマップの背景
            /// </summary>
            Base = 13,

            /// <summary>
            /// プログラムマップの背景（実行のスタート地点を示す）
            /// </summary>
            BaseStart = 14,

            /// <summary>
            /// プログラムマップの背景（実行中の地点を示す）
            /// </summary>
            BaseActive = 15,


            //プログラムマップに配置したチップの「次の実行先」を示すタイル
            //分岐チップに関しては「壁がある場合の実行先」を示す
            //ProgramEditorのprogramArrowMap（programArrowTilemap）で扱う

            /// <summary>
            /// 赤矢印：上
            /// </summary>
            RedArrowUp = 16,

            /// <summary>
            /// 赤矢印：右
            /// </summary>
            RedArrowRight = 17,

            /// <summary>
            /// 赤矢印：下
            /// </summary>
            RedArrowDown = 18,

            /// <summary>
            /// 赤矢印：左
            /// </summary>
            RedArrowLeft = 19,


            //プログラムマップに配置した分岐チップの「壁がない場合の実行先」を示すタイル
            //行動チップには無関係
            //ProgramEditorのprogramIfArrowMap（programIfArrowTilemap）で扱う

            /// <summary>
            /// 青矢印：上
            /// </summary>
            BlueArrowUp = 20,

            /// <summary>
            /// 青矢印：右
            /// </summary>
            BlueArrowRight = 21,

            /// <summary>
            /// 青矢印：下
            /// </summary>
            BlueArrowDown = 22,

            /// <summary>
            /// 青矢印：左
            /// </summary>
            BlueArrowLeft = 23,


            //汎用のタイル

            /// <summary>
            /// タイルが何もない状態を明示する時に使う
            /// </summary>
            Empty = 24
        };


        /// <summary>
        /// 上下左右の向きに対応するIDを設定
        /// 上向きから90度ずつ、右回りに順番が設定されているイメージ
        /// (他のプログラムで、この順番であることを利用して向きを計算している場合がある)
        /// </summary>
        public enum DirectionId{
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        };
        //
        /// <summary>
        /// 向きIDリストの要素数
        /// </summary>
        public static int DirectionLength = Enum.GetNames(typeof(DirectionId)).Length;

        /// <summary>
        /// ゲーム内のプログラムマップの大きさを設定
        /// 正方形の一辺のマス数で指定する
        /// </summary>
        public const int ProgramMapSize = 3;
    }

}