using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Threading;

namespace Basic
{
  
  public partial class Replys : Form
  {
    private HttpClient client;
    private string account;
    private int commentId;
    private int poemId;
    private string name;
    private string baseUrl;
    private XmlNode Node;
    private bool alreadyPrase;
    public class newReply
    {

      public string UserAccount { get; set; }
      public int CommentId { get; set; }
      public string Content { get; set; }
      public DateTime created { get; set; }
      
      public newReply(string account,string content, int commentId)
      {

        UserAccount = account;
        Content = content;
        CommentId= commentId;
        created = DateTime.Now;
      }
      public override string ToString()

      {

        return $" {UserAccount}:{CommentId}:{Content}:{created}";

      }
    }
    public Replys(string account,int Id,string name,int poemId)
    {
      InitializeComponent();
      XmlDocument serverDoc = new XmlDocument();
      serverDoc.Load("../../../serverIp.xml");
      Node = serverDoc.SelectSingleNode("serverIp");
      HttpClientHandler handler = new HttpClientHandler();
      handler.ServerCertificateCustomValidationCallback = (message, cert, chain, err) => true;
      client = new HttpClient(handler);
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
      this.account = account;
      commentId = Id;
      this.poemId = poemId;
      this.name = name;
      HttpGet();
      uiLabel1.Text = "评论"+commentId + "的回复";
    }
    public void HttpGet()
    {
      Comment a = new Comment();
      var task = client.GetStringAsync("https://"+Node.InnerText+":5001/api/comment?poemId=" + poemId);
      var model =
          JsonConvert.DeserializeObject<List<Comment>>(task.Result);
      var query = from s1 in model
                  where commentId == s1.Id
                  select s1;
      foreach (var comment in query)
      {
        a = comment;
      }
      replyBindingSource.DataSource = a.Replys;
      uiDataGridView1.DataSource = replyBindingSource;
    }
    public void JudePrase()
    {
      Reply reply = (Reply)replyBindingSource.Current;
      var task1 = client.GetStringAsync("https://" + Node.InnerText+ ":5001/api/replyprase?account=" + account + "&Id=" + reply.Id);
      alreadyPrase = Boolean.Parse(task1.Result);
      //MessageBox.Show(""+alreadyPrase);
      if (alreadyPrase)
      {
        uiButton2.Text = "已赞";
      }
      else uiButton2.Text = "赞";
    }
    private void uiDataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
      JudePrase();
    }

    private void uiLabel1_Click(object sender, EventArgs e)
    {

    }

    private void uiButton1_Click(object sender, EventArgs e)
    {
      if (uiTextBox1.Text == "")
      {
        MessageBox.Show("回复不能为空！");
      }
      else
      {
        var myReply = new newReply(account, uiTextBox1.Text, commentId);
        var json = JsonConvert.SerializeObject(myReply);
        HttpContent data = new StringContent(json);
        data.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        var task = client.PostAsync("https://"+Node.InnerText+":5001/api/reply", data);
        Thread.Sleep(100);
        HttpGet();
      }

    }

    private void uiPanel1_Click(object sender, EventArgs e)
    {

    }

    private void uiButton2_Click(object sender, EventArgs e)
    {
      if (uiButton2.Text == "已赞")
      {
        Reply reply = (Reply)replyBindingSource.Current;
        var task1 = client.DeleteAsync("https://" + Node.InnerText + ":5001/api/replyprase?account=" + account + "&Id=" + reply.Id);
        reply.prase = reply.prase - 1;
        var json = JsonConvert.SerializeObject(reply);
        //MessageBox.Show(json);
        HttpContent data = new StringContent(json);
        data.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        var task2 = client.PutAsync("https://" + Node.InnerText + ":5001/api/reply/" + reply.Id, data);
        Thread.Sleep(100);
        HttpGet();
        JudePrase();
      }
      else
      {
        Reply reply = (Reply)replyBindingSource.Current;
        var task1 = client.PostAsync("https://" + Node.InnerText + ":5001/api/replyprase?account=" + account + "&replyId=" + reply.Id, null);
        reply.prase = reply.prase + 1;
        var json = JsonConvert.SerializeObject(reply);
        //MessageBox.Show(json);
        HttpContent data = new StringContent(json);
        data.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        var task2 = client.PutAsync("https://" + Node.InnerText + ":5001/api/reply/" + reply.Id, data);
        Thread.Sleep(100);
        HttpGet();
        JudePrase();
      }
    }
  }
}
