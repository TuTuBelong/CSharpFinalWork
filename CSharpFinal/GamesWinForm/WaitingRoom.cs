﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client;

namespace GameWinForm
{
    public partial class WaitingRoom : Form
    {
        public BindingList<Player> PlayerList = new BindingList<Player>();
        public Client.Client client;
        public int RoomID { set; get; }
        public string RoomName { set; get; }
        public int Type { set; get; }
        public int PlayerReadyNum { set; get; } = 0;

        protected Regex regex = new Regex(@"\w+");
        public  Action<string> OnShowGameForm =s=> { };
        public RoomForm parent { get; set; }

        public WaitingRoom(Client.Client c, int roomID, string roomName, int type, string message,RoomForm parent)
        {
            client = c;
            c.StartReceive();

            if (message != "")
            {
                MatchCollection matches = regex.Matches(message);
                for (int i = 0; 2 * i < matches.Count; i++)
                {
                    if (matches[i + 1].Value == "已准备") PlayerReadyNum++;
                    AddToPlayerList(new Player(matches[i].Value, matches[i + 1].Value));
                }
            }

            InitializeComponent();
            this.parent = parent;
            RoomID = roomID;
            RoomName = roomName;
            Type = type;

            //数据绑定
            this.uiLabelName.Text = RoomName;

            OnShowGameForm += ShowGameForm;

            
            client.PlayerGetIn = (str) =>
            {
                if (this.uiDataGridViewPlayers.InvokeRequired)
                {
                    this.Invoke(new Action(delegate ()
                    {
                        AddToPlayerList(new Player(str, "未准备"));
                        this.uiDataGridViewPlayers.DataSource = PlayerList;
                    }));
                }
                else
                {
                    AddToPlayerList(new Player(str, "未准备"));
                    this.uiDataGridViewPlayers.DataSource = PlayerList;
                }

            };
            client.PlayerGetReady = (str) =>
            {
                //准备人数加一
                PlayerReadyNum++;
                //更新界面
                this.Invoke(new Action(delegate ()
                {
                    ChangeStateToReady(str);
                    this.uiDataGridViewPlayers.Refresh();
                }));
                //判断是否开始游戏
                if (PlayerReadyNum >= PlayerList.Count && PlayerReadyNum > 1) 
                {
                    OnShowGameForm(str);
                }

            };
            client.PlayerQuitRoom = (str) =>
            {
                if (this.uiDataGridViewPlayers.InvokeRequired)
                {
                    this.Invoke(new Action(delegate ()
                    {
                        RemovePlayer(str);
                        this.uiDataGridViewPlayers.Refresh();
                    }));
                }
                else
                {
                    RemovePlayer(str);
                    this.uiDataGridViewPlayers.Refresh();
                }
            };

        }

        private void uiButtonReady_Click(object sender, EventArgs e)
        {
            client.GetReady();
            this.uiButtonReady.Enabled = false;
        }

        public void ShowGameForm(string str)
        {
            List<string> list = new List<string>();
            foreach(Player player in PlayerList)
            {
                list.Add(player.Name);
            }

            this.Invoke(new Action(delegate ()
            {
                if (Type == 0)
                {
                    FlyFlowerForm form = new FlyFlowerForm((FlyingFlowerClient)client, list);
                    this.Hide();
                    form.Show();
                    form.FormClosing += (x, y) => parent.Close();
                    this.Dispose();
                }
                else if(Type == 1)
                {
                    DrawAndGuess form = new DrawAndGuess((DrawAndGuessClient)client, list);
                    this.Hide();
                    form.Show();
                    form.FormClosing += (x, y) =>
                    {
                        parent.Close();
                        this.client.QuitRoom();
                    };
                    this.Dispose();
                }
            }));
        }

        //添加玩家到PlayerList
        public void AddToPlayerList(Player player)
        {
            lock (PlayerList)
            {
                PlayerList.Add(player);
            }
        }
        //移除玩家
        public void RemovePlayer(string str)
        {
            lock (PlayerList)
            {
                Player player = PlayerList.FirstOrDefault((p) => p.Name == str);
                if (player.State == "已准备") PlayerReadyNum--;
                PlayerList.Remove(player);
            }
        }
        //修改PlayerList中某玩家的状态为准备
        public void ChangeStateToReady(string name)
        {
            lock (PlayerList)
            {
                PlayerList.FirstOrDefault((player) => player.Name == name).State = "已准备";
            }
        }
        //窗口关闭事件，玩家退出房间
        private void Quit(object sender, FormClosingEventArgs e)
        {
            client.QuitRoom();
            e.Cancel = true;
            this.Dispose();
        }
    }
    public class Player
    {
        public string Name { get; set; }
        public string State { get; set; }

        public Player(string name, string state)
        {
            Name = name;
            State = state;
        }
    }
}
