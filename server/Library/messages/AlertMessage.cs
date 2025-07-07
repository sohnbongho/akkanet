using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.messages
{
    /// <summary>
    /// 알림 종류
    /// </summary>
    public enum AlertType : int
    {
        None = 0,
        ArticleAdded = 1,                       // 게시글 추가
        ArticleComment = 2,                     // 게시글에 댓글 추가
        ArticleCommentReply = 3,                // 댓글에 댓글
        ArticleReportedDeleted = 4,             // 신고에 의한 삭제
        ArticleLiked = 5,                           // 게시글 좋아요 추가
        ArticleCommentReportedDeleted = 6,       // 신고에 의한 삭제
        
        ShopGiftItem = 7,                       // 선물하기 알림

    }

    public interface IAlertMessage
    {
        string FromUserSeq { get; set; }
        string FromCharSeq { get; set; }
        string FromUserName { get; set; }
        string FromImageUrl { get; set; }
    }

    /// <summary>
    /// 글이 추가됨
    /// </summary>
    public class ArticleCreated : IAlertMessage
    {
        public string FromUserSeq { get; set; } = string.Empty; 
        public string FromCharSeq { get; set; } = string.Empty;
        public string FromUserName { get; set; } = string.Empty; 
        public string FromImageUrl { get; set; } = string.Empty; 

        public string ArticleId { get; set; } = string.Empty; // 게시글 ID
        public string JsonData { get; set; } = string.Empty; // 게시글 내용
    }    

    /// <summary>
    /// 댓글 달림
    /// </summary>
    public class ArticleComment : IAlertMessage
    {
        public string FromUserSeq { get; set; } = string.Empty; 
        public string FromCharSeq { get; set; } = string.Empty;
        public string FromUserName { get; set; } = string.Empty; 
        public string FromImageUrl { get; set; } = string.Empty; 

        public string ArticleId { get; set; } = string.Empty; // 게시글 ID
        public string CommentId { get; set; } = string.Empty; // 댓글ID
        public string CommentJsonData { get; set; } = string.Empty; // 댓글 내용
    }

    /// <summary>
    /// 댓글의 댓글 달림
    /// </summary>
    public class ArticleCommentReply : IAlertMessage
    {
        public string FromUserSeq { get; set; } = string.Empty; 
        public string FromCharSeq { get; set; } = string.Empty;
        public string FromUserName { get; set; } = string.Empty; 
        public string FromImageUrl { get; set; } = string.Empty; 

        public string ArticleId { get; set; } = string.Empty; // 게시글 ID
        public string CommentReplyId { get; set; } = string.Empty; // 대댓글ID
        public string CommentReplyJsonData { get; set; } = string.Empty; // 대댓글 내용
    }

    /// <summary>
    /// 글이 삭제 되었음
    /// </summary>
    public class ArticleDeleted : IAlertMessage
    {
        public enum Reason
        {
            None = 0,
            Reported = 1,   // 신고에 의한 삭제
        }
        public string FromUserSeq { get; set; } = string.Empty; 
        public string FromCharSeq { get; set; } = string.Empty;
        public string FromUserName { get; set; } = string.Empty; 
        public string FromImageUrl { get; set; } = string.Empty; 

        public Reason DeletedReason { get; set; } = Reason.None;
        public string ArticleId { get; set; } = string.Empty; // 게시글 ID
        public string JsonData { get; set; } = string.Empty; // 게시글 내용
    }

    /// <summary>
    /// 글이 삭제 되었음
    /// </summary>
    public class ArticleCommentDeleted : IAlertMessage
    {
        public enum Reason
        {
            None = 0,
            Reported = 1,   // 신고에 의한 삭제
        }
        public string FromUserSeq { get; set; } = string.Empty; 
        public string FromCharSeq { get; set; } = string.Empty;
        public string FromUserName { get; set; } = string.Empty;
        public string FromImageUrl { get; set; } = string.Empty;

        public Reason DeletedReason { get; set; } = Reason.None;
        public string ArticleId { get; set; } = string.Empty; // 게시글 ID
        public string CommentId { get; set; } = string.Empty; // 댓글 ID
        public string JsonData { get; set; } = string.Empty; // 게시글 내용
    }

    /// <summary>
    /// 좋아요 추가
    /// </summary>
    public class ArticleLiked : IAlertMessage
    {
        public string FromUserSeq { get; set; } = string.Empty; 
        public string FromCharSeq { get; set; } = string.Empty;
        public string FromUserName { get; set; } = string.Empty;
        public string FromImageUrl { get; set; } = string.Empty;

        public string ArticleId { get; set; } = string.Empty; // 게시글 ID
        public string ArticleJson { get; set; } = string.Empty; // 게시글 내용
        public string LikedNickName { get; set; } = string.Empty; // 좋아요 누른 유저        
    }

    
    /// <summary>
    /// 선물하기 타입
    /// </summary>
    public enum MailType : int
    {
        None = 0,
        ShopPresent = 1,   //상점에서 선물하기
        GmTool = 2,   // GmTool로 지급
    }
    public interface IPresent
    {        
    }

    /// <summary>
    /// 상점 아이템 선물하기
    /// </summary>
    public class ShopGiftItem : IPresent
    {
        public string FromUserSeq { get; set; } = string.Empty; // 선물한 유저
        public string FromUserName { get; set; } = string.Empty; // 선물한 유저 이름
        public string FromImageUrl { get; set; } = string.Empty; // 선물한 유저 프로필
        public string TargetUserSeq { get; set; } = string.Empty; // 선물한 유저 이름
        public string TargetUserName { get; set; } = string.Empty; // 선물한 유저 이름
        public int ItemId { get; set; }
        public int ItemAmount { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class GmGiftItem : IPresent
    {        
        public string TargetUserSeq { get; set; } = string.Empty; // 선물한 유저 이름        
        public int ItemId { get; set; }
        public int ItemAmount { get; set; }
        public string Text { get; set; } = string.Empty;
    }


}
