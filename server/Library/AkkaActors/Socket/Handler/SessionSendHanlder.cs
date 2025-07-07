using Akka.IO;
using Library.Helper.Encrypt;
using Library.Memory.ProtoBuffer;
using log4net;
using Messages;
using System.Reflection;

namespace Library.AkkaActors.Socket.Handler;


public class SessionSendHanlder : ISessionSendHanlder, IDisposable
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public SessionSendHanlder()
    {
    }
    public void Init()
    {

    }
    public void Dispose()
    {

    }

    public bool Tell(SessionInfoHandler session, MessageWrapper request)
    {

        try
        {
            var encrypt = session.PacketEncrypt;
            var requestBinary = request.ToByteArrayWithBuffer();
            request.MessageSize = requestBinary.Length;

            ushort totalSize = sizeof(ushort);
            ushort messageSize = (ushort)requestBinary.Length;

            byte[] binary = null!;

            if (encrypt)
            {
                binary = CryptographyHelper.EncryptPacket(requestBinary);
                totalSize += (ushort)binary.Length;
            }
            else
            {
                binary = requestBinary;
                totalSize += (ushort)requestBinary.Length;
            }

            byte[] byteArray = null!;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(totalSize); // 
                    writer.Write(messageSize);
                    writer.Write(binary);

                    byteArray = stream.ToArray();
                }
            }
            if (byteArray != null)
                session.TellConnectedSocket(Tcp.Write.Create(ByteString.FromBytes(byteArray)));

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("failed to Tell", ex);
            return false;
        }
    }   
}
