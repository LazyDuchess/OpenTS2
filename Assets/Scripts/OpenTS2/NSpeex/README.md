# NSpeex
>早在11-12年，由于一个游戏项目要使用到语音压缩库，当时选用了speex,我们当时使用的是Unity3D游戏引擎，由于市面上没有.Net平台下的speex的库，于是我便将jspeex翻译成NSpeex。但是后来由于种种原因不再维护项目，前段时间有人问我这个库该怎么用，我就使用别人的[NSpeex](http://nspeex.codeplex.com/)库来进行解释说明，毕竟我之前的代码太像Java风格了，于是我将别人的代码复制过来，自己写点文档方便别人
## 例子(example)
见 Test/SpeexTest.cs,将male.wav 转化为result.spx文件，然后使用foobar播放器可以播放

## 编码(encode)
```
// byte[] dataIn -> byte[] dataOut
// convert to short
short[] data = new short[dataIn.Length / 2];
Buffer.BlockCopy(dataIn, 0, data, 0, dataIn.Length);
byte[] dataOut = new byte[dataIn.Length];
int encSize = speexEncoder.Encode(data, 0, data.Length, dataOut, 0, dataOut.Length);
if(encSize > 0){
    // encode success
    // todo with dataOut[0-encSize]
}

```
## 解码(decode)
```
// byte[] dataIn -> byte[] dataOut
short[] data =  new short[1024];
int decSize = speexEncoder.Decode(dataIn, 0, dataIn.Length, data, 0, false);

if(decSize > 0 ){
    byte[] dataOut = new byte[1024];
    Buffer.BlockCopy(data, 0, dataOut, 0, decSize);
    // todo with dataOut[0-decSize]
}
```