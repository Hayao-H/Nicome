using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Nicome.WWW.API.Types
{
    namespace WatchPage
    {
        public class User
        {
            public string? id { get; set; }
            public string? nickname { get; set; }
        }

        public class UserMe
        {
            public int user_id { get; set; }
            public string? nickname { get; set; }
        }

        public class BaseJson
        {
            public Video? video { get; set; }
            public User? owner { get; set; }
            public int isOfficialAnime { get; set; } = 0;
            public Context? context { get; set; }
            public CommentComposite? commentComposite { get; set; }
            public ThreadRoot? thread { get; set; }
        }

        public class Video
        {
            public string? id { get; set; }
            public string? title { get; set; }
            public int? isOfficialAnime { get; set; } = 0;
            public string? postedDateTime { get; set; }
            public Dmcinfo? dmcInfo { get; set; }
            public float duration { set; get; }
        }

        public class Dmcinfo
        {
            public Encryption? encryption { get; set; }
            public string? tracking_id { get; set; }
            public Session_Api? session_api { get; set; } = new Session_Api();
            public UserMe? user { get; set; }
            public Thread? thread { get; set; }
        }
        public class Encryption
        {
            public Hls_Encryption_V1? hls_encryption_v1 { get; set; }
        }

        public class Hls_Encryption_V1
        {
            public string? encrypted_key { get; set; }
            public string? key_uri { get; set; }
        }

        public class Session_Api
        {
            public string? recipe_id { get; set; }
            public string? player_id { get; set; }
            public List<string>? videos { get; set; }
            public string?[]? audios { get; set; }
            public Auth_Types? auth_types { get; set; }
            public string? service_user_id { get; set; }
            public string? token { get; set; }
            public string? signature { get; set; }
            public string? content_id { get; set; }
            public int? heartbeat_lifetime { get; set; }
            public int? content_key_timeout { get; set; }
            public float priority { get; set; }
        }

        public class Auth_Types
        {
            public string? hls { get; set; }
            public string? http { get; set; }
        }

        public class Context
        {
            public bool isVideoOwner { get; set; }
            public bool isThreadOwner { get; set; }
            public string? csrfToken { get; set; }
            public string? userkey { get; set; }
        }
        public class Thread
        {
            public int thread_id { get; set; }
        }
        public class ThreadRoot
        {
            public uint commentCount { get; set; }
        }
        public class CommentComposite
        {
            public List<CommentCompositeItems.Thread>? threads { get; set; }
        }

        namespace CommentCompositeItems
        {
            public class Thread
            {
                public int id { get; set; }
                public int fork { get; set; }
                public bool isActive { get; set; }
                public int postkeyStatus { get; set; }
                public bool isDefaultPostTarget { get; set; }
                public bool isThreadkeyRequired { get; set; }
                public bool isLeafRequired { get; set; }
                public string? label { get; set; }
                public bool isOwnerThread { get; set; }
                public bool hasNicoscript { get; set; }

                public class Layer
                {
                    public int index { get; set; }
                    public bool isTranslucent { get; set; }
                    public List<Threadid>? threadIds { get; set; }
                }

                public class Threadid
                {
                    public int id { get; set; }
                    public int fork { get; set; }
                }
            }

        }
    }

    namespace Comment
    {
        public class RequestItems
        {
            public CommentItems.Thread? thread { get; set; }
            public CommentItems.Ping? ping { get; set; }
            public CommentItems.Leaf? thread_leaves { get; set; }
        }

        namespace CommentItems
        {
            public class Thread
            {
                public string? thread { get; set; }
                public int fork { get; set; }
                public string version { get; set; } = "20090904";
                public int language { get; set; } = 0;
                public string? user_id { get; set; }
                public int with_global { get; set; } = 1;
                public int scores { get; set; } = 0;
                public int nicoru { get; set; } = 3;
                public string? userkey { get; set; }
                public string? force_184 { get; set; }
                public string? threadkey { get; set; }
                [JsonPropertyName("when")]
                public int? whencom { get; set; } = null;
                public string? waybackkey { get; set; }
            }

            public class Ping
            {
                public string? content { get; set; }
            }

            public class Leaf
            {
                public string? thread { get; set; }
                public int language { get; set; } = 0;
                public string? user_id { get; set; }
                public string? content { get; set; }
                public int scores { get; set; } = 1;
                public int nicoru { get; set; } = 3;
                public string? userkey { get; set; }
                public string? force_184 { get; set; }
                public string? threadkey { get; set; }
                [JsonPropertyName("when")]
                public int? whencom { get; set; } = null;
                public string? waybackkey { get; set; }

            }
        }

        namespace CommentBody
        {
            namespace Json
            {

                public class JsonComment : IEquatable<JsonComment>
                {
                    public Thread? thread { get; set; }
                    public Chat? chat { get; set; }


                    #pragma warning disable CS8769
                    #pragma warning disable CS8767
                    public bool Equals(JsonComment other)
                    #pragma warning restore CS8767
                    #pragma warning restore CS8769
                    {
                        if (thread != null && other.thread != null)
                        {
                            return thread.thread==other.thread.thread;
                        }
                        else if (chat != null && other.chat != null)
                        {
                            return chat.no==other.chat.no;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    public override int GetHashCode()
                    {
                        if (thread != null)
                        {
                            return thread.thread.GetHashCode();
                        } else if (chat!=null)
                        {
                            return chat.no.GetHashCode();
                        } else
                        {
                            return 1;
                        }
                    }
                }

                public class Chat
                {
                    public int GetPrevDate()
                    {
                        return this.date - 1;
                    }
                    public string? thread { get; set; }
                    public int no { get; set; }
                    public int vpos { get; set; }
                    public int leaf { get; set; }
                    public int date { get; set; }
                    public int score { get; set; }
                    public int nicoru { get; set; }
                    public string? last_nicoru_date { get; set; }
                    public int premium { get; set; }
                    public int anonymity { get; set; }
                    public string? user_id { get; set; }
                    public string? mail { get; set; }
                    public string? content { get; set; }
                    public int date_usec { get; set; }
                    public int deleted { get; set; }
                    public string? force_184 { get; set; }
                    public string? threadkey { get; set; }
                }

                public class Thread
                {
                    public int resultcode { get; set; }
                    public string thread { get; set; } = "";
                    public int server_time { get; set; }
                    public int last_res { get; set; }
                    public string? ticket { get; set; }
                    public int revision { get; set; }
                    public int click_revision { get; set; }
                    public int fork { get; set; }
                }


            }

            namespace XML
            {

                /// <remarks/>
                [System.SerializableAttribute()]
                [System.ComponentModel.DesignerCategoryAttribute("code")]
                [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
                [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
                public partial class packet
                {

                    private packetThread? threadField;

                    private List<packetChat>? chatField;

                    /// <remarks/>
                    public packetThread thread
                    {
                        get
                        {
                            return this.threadField;
                        }
                        set
                        {
                            this.threadField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlElementAttribute("chat")]
                    public List<packetChat> chat
                    {
                        get
                        {
                            return this.chatField;
                        }
                        set
                        {
                            this.chatField = value;
                        }
                    }
                }

                /// <remarks/>
                [System.SerializableAttribute()]
                [System.ComponentModel.DesignerCategoryAttribute("code")]
                [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
                public partial class packetThread
                {

                    private int resultcodeField;

                    private int threadField;

                    private int server_timeField;

                    private int last_resField;

                    private string? ticketField;

                    private int revisionField;

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int resultcode
                    {
                        get
                        {
                            return this.resultcodeField;
                        }
                        set
                        {
                            this.resultcodeField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int thread
                    {
                        get
                        {
                            return this.threadField;
                        }
                        set
                        {
                            this.threadField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int server_time
                    {
                        get
                        {
                            return this.server_timeField;
                        }
                        set
                        {
                            this.server_timeField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int last_res
                    {
                        get
                        {
                            return this.last_resField;
                        }
                        set
                        {
                            this.last_resField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public string ticket
                    {
                        get
                        {
                            return this.ticketField;
                        }
                        set
                        {
                            this.ticketField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int revision
                    {
                        get
                        {
                            return this.revisionField;
                        }
                        set
                        {
                            this.revisionField = value;
                        }
                    }
                }

                /// <remarks/>
                [System.SerializableAttribute()]
                [System.ComponentModel.DesignerCategoryAttribute("code")]
                [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
                public partial class packetChat
                {

                    private int threadField;

                    private int noField;

                    private int vposField;

                    private int dateField;

                    private int date_usecField;

                    private int premiumField;


                    private int anonymityField;

                    private string? user_idField;

                    private string? mailField;

                    private int leafField;

                    private bool leafFieldSpecified;

                    private int deletedField;


                    private int scoreField;


                    private string? valueField;

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int thread
                    {
                        get
                        {
                            return this.threadField;
                        }
                        set
                        {
                            this.threadField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int no
                    {
                        get
                        {
                            return this.noField;
                        }
                        set
                        {
                            this.noField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int vpos
                    {
                        get
                        {
                            return this.vposField;
                        }
                        set
                        {
                            this.vposField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int date
                    {
                        get
                        {
                            return this.dateField;
                        }
                        set
                        {
                            this.dateField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int date_usec
                    {
                        get
                        {
                            return this.date_usecField;
                        }
                        set
                        {
                            this.date_usecField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int premium
                    {
                        get
                        {
                            return this.premiumField;
                        }
                        set
                        {
                            this.premiumField = value;
                        }
                    }


                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int anonymity
                    {
                        get
                        {
                            return this.anonymityField;
                        }
                        set
                        {
                            this.anonymityField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public string user_id
                    {
                        get
                        {
                            return this.user_idField;
                        }
                        set
                        {
                            this.user_idField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public string mail
                    {
                        get
                        {
                            return this.mailField;
                        }
                        set
                        {
                            this.mailField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int leaf
                    {
                        get
                        {
                            return this.leafField;
                        }
                        set
                        {
                            this.leafField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlIgnoreAttribute()]
                    public bool leafSpecified
                    {
                        get
                        {
                            return this.leafFieldSpecified;
                        }
                        set
                        {
                            this.leafFieldSpecified = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int deleted
                    {
                        get
                        {
                            return this.deletedField;
                        }
                        set
                        {
                            this.deletedField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlAttributeAttribute()]
                    public int score
                    {
                        get
                        {
                            return this.scoreField;
                        }
                        set
                        {
                            this.scoreField = value;
                        }
                    }

                    /// <remarks/>
                    [System.Xml.Serialization.XmlTextAttribute()]
                    public string Value
                    {
                        get
                        {
                            return this.valueField;
                        }
                        set
                        {
                            this.valueField = value;
                        }
                    }
                }


            }
        }
    }
}
