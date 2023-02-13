using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WebAdmin.Models
{
    /*Kanban*/
    public class Encryption
    {
        private byte[] FIV;
        private byte[] FKEY;

        private string sKey;

        public Encryption()
        {
            this.DefaultInit();

            sKey = "20170718";
        }

        /// <summary>
        /// 默认键值和初始向量
        /// </summary>
        private void DefaultInit()
        {
            byte[] IV = { 19, 78, 0, 4, 22, 99, 234, 36 };
            byte[] key = { 19, 80, 0, 9, 20, 4, 218, 132 };

            FIV = IV;
            FKEY = key;
        }

        /// <summary>
        /// 采用标准 64位 DES 算法加密
        /// </summary>
        /// <param name="strText">要加密的字符串</param>
        /// <returns>返回加密后的字符串</returns>
        public string Encrypt(string strText)
        {
            //访问数据加密标准(DES)算法的加密服务提供程序 (CSP) 版本的包装对象
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);　//建立加密对象的密钥和偏移量
            des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);　 //原文使用ASCIIEncoding.ASCII方法的GetBytes方法

            byte[] inputByteArray = Encoding.Default.GetBytes(strText);//把字符串放到byte数组中

            MemoryStream ms = new MemoryStream();//创建其支持存储区为内存的流　
            //定义将数据流链接到加密转换的流
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            //上面已经完成了把加密后的结果放到内存中去

            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            ret.ToString();
            return ret.ToString();
        }


        /// <summary>
        /// 标准64位DES解密
        /// </summary>
        /// <param name="strText">要解密的字符串</param>
        /// <returns>返回解密后的字符串</returns>
        public string Decrypt(string strText)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            byte[] inputByteArray = new byte[strText.Length / 2];
            for (int x = 0; x < strText.Length / 2; x++)
            {
                int i = (Convert.ToInt32(strText.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);　//建立加密对象的密钥和偏移量，此值重要，不能修改
            des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);

            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            //建立StringBuild对象，createDecrypt使用的是流对象，必须把解密后的文本变成流对象
            StringBuilder ret = new StringBuilder();

            return System.Text.Encoding.Default.GetString(ms.ToArray());
        }
    }
}