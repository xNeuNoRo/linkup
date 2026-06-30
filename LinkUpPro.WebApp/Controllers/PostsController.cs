using LinkUpPro.Application.DTOs.Comment.Requests;
using LinkUpPro.Application.DTOs.Comment.Responses;
using LinkUpPro.Application.DTOs.Reaction.Requests;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Application.ViewModels.CommentViewModels;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Enums;
using LinkUpPro.WebApp.Filters;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.WebApp.Controllers;

/// <summary>
/// Controlador para interacciones con publicaciones:
/// Comentarios, Respuestas, Reacciones.
/// </summary>
[SessionAuthorize]
public class PostsController : BaseController
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly IReactionService _reactionService;
    private readonly ILogger<PostsController> _logger;

    public PostsController(
        IPostService postService,
        ICommentService commentService,
        IReactionService reactionService,
        ICurrentUserService currentUserService,
        ILogger<PostsController> logger
    )
        : base(currentUserService)
    {
        _postService = postService;
        _commentService = commentService;
        _reactionService = reactionService;
        _logger = logger;
    }

    // ====================== COMMENTS ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(CreateCommentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ShowError("El comentario no es válido.");
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var request = model.Adapt<CreateCommentRequest>();
            var result = await _commentService.CreateAsync(_currentUserService.UserId!, request);

            if (!result.IsSuccess)
            {
                ShowError(result.Error?.Message ?? "No se pudo agregar el comentario.");
            }
            else
            {
                ShowAlert("Comentario agregado.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error agregando comentario");
            ShowError("No se pudo agregar el comentario.");
        }

        return RedirectToAction("Index", "Home", new { highlightPostId = model.PostId });
    }

    // ====================== REPLIES ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReply(CreateReplyViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ShowError("La respuesta no es válida.");
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var request = model.Adapt<CreateReplyRequest>();
            var result = await _commentService.CreateReplyAsync(_currentUserService.UserId!, request);

            if (!result.IsSuccess)
            {
                ShowError(result.Error?.Message ?? "No se pudo agregar la respuesta.");
            }
            else
            {
                ShowAlert("Respuesta agregada.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error agregando respuesta");
            ShowError("No se pudo agregar la respuesta.");
        }

        return RedirectToAction("Index", "Home", new { highlightPostId = model.PostId });
    }

    // ====================== EDIT COMMENT ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditComment(long id, string content)
    {
        try
        {
            var request = new UpdateCommentRequest(content);
            var result = await _commentService.UpdateAsync(_currentUserService.UserId!, id, request);

            if (!result.IsSuccess)
            {
                ShowError(result.Error?.Message ?? "No se pudo editar el comentario.");
            }
            else
            {
                ShowAlert("Comentario editado.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error editando comentario {CommentId}", id);
            ShowError("No se pudo editar el comentario.");
        }

        return RedirectToAction("Index", "Home");
    }

    // ====================== DELETE COMMENT ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(long id)
    {
        try
        {
            var result = await _commentService.DeleteAsync(_currentUserService.UserId!, id);
            if (!result.IsSuccess)
            {
                ShowError(result.Error?.Message ?? "No se pudo eliminar el comentario.");
            }
            else
            {
                ShowAlert("Comentario eliminado.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error eliminando comentario {CommentId}", id);
            ShowError("No se pudo eliminar el comentario.");
        }

        return RedirectToAction("Index", "Home");
    }

    // ====================== LOAD MORE REPLIES (AJAX) ======================

    [HttpGet]
    public async Task<IActionResult> GetReplies(long parentCommentId, int page = 1)
    {
        var userId = _currentUserService.UserId!;
        var result = await _commentService.GetCommentRepliesAsync(userId, parentCommentId, page, 5);

        if (!result.Items.Any())
            return Content("");

        var viewModels = result.Items
            .Select(dto => MapCommentTreeToViewModel(dto, userId, canReply: true))
            .ToList();

        ViewData["ParentCommentId"] = parentCommentId;
        ViewData["CurrentPage"] = page;
        ViewData["HasMore"] = result.TotalCount > page * 5;

        return PartialView("_CommentRepliesPartial", viewModels);
    }

    // ====================== REPLY FORM LAZY-LOAD (AJAX) ======================

    [HttpGet]
    public IActionResult ReplyForm(long parentCommentId, long postId)
    {
        var vm = new CreateReplyViewModel { PostId = postId, ParentCommentId = parentCommentId };
        return PartialView("_CommentForm", vm);
    }

    // ====================== PRIVATE MAPPERS ======================

    private static CommentViewModel MapCommentTreeToViewModel(
        CommentTreeDto node,
        string currentUserId,
        bool canReply
    )
    {
        var c = node.Comment;
        var vm = new CommentViewModel
        {
            Id = c.Id,
            PostId = c.PostId,
            ParentCommentId = c.ParentCommentId,
            AuthorId = c.AuthorId,
            AuthorName = c.AuthorName,
            AuthorProfilePicture = c.AuthorProfilePicture,
            Content = c.Content,
            IsEdited = c.IsEdited,
            CreatedAt = c.CreatedAt.UtcDateTime,
            UpdatedAt = c.UpdatedAt?.UtcDateTime,
            RepliesCount = node.TotalRepliesCount,
            HasMoreReplies = node.HasMoreReplies,
            Replies = node.Replies.Select(r => MapCommentTreeToViewModel(r, currentUserId, canReply && !c.IsDeleted)).ToList(),
            IsDeleted = c.IsDeleted,
            IsOwn = c.AuthorId == currentUserId,
            CanReply = canReply && !c.IsDeleted
        };
        return vm;
    }

    // ====================== REACTIONS ======================

    /// <summary>
    /// Registra, cambia o elimina una reacción del usuario a una publicación.
    /// POST /Posts/ReactPost
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReactPost(long postId, int reactionType)
    {
        if (reactionType < 0 || reactionType > 2)
        {
            return Json(new { success = false, message = "Tipo de reacción no válido." });
        }

        try
        {
            // 0 = quitar reacción
            if (reactionType == 0)
            {
                var deleteResult = await _reactionService.DeleteAsync(_currentUserService.UserId!, postId);
                if (!deleteResult.IsSuccess)
                    return Json(new { success = false, message = deleteResult.Error?.Message ?? "Error" });
            }
            else
            {
                var request = new CreateReactionRequest(postId, reactionType);
                var result = await _reactionService.ReactAsync(_currentUserService.UserId!, request);
                if (!result.IsSuccess)
                    return Json(new { success = false, message = result.Error?.Message ?? "Error" });
            }

            var counts = await _reactionService.GetCountsAsync(postId);
            return Json(new
            {
                success = true,
                likes = counts.Value?.Likes ?? 0,
                dislikes = counts.Value?.Dislikes ?? 0,
                userReaction = reactionType
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error en reacción {PostId}", postId);
            return Json(new { success = false, message = "Error al procesar la reacción." });
        }
    }
}
