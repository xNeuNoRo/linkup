using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Infrastructure.Persistence.Repositories;

namespace LinkUpPro.Tests.Infrastructure.Repositories;

public sealed class NotificationRepositoryTests : PersistenceTestBase
{
    [Fact]
    public async Task GetByRecipientAsync_ReturnsRecipientNotifications()
    {
        var context = CreateContext();
        var repo = new NotificationRepository(context);
        var actor = await SeedUserAsync(context, "actor", "actor@t.com");
        var recipient = await SeedUserAsync(context, "recipient", "recip@t.com");

        var post = Post.Create(recipient.Id, "Post", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var notif = Notification.CreateComment(recipient.Id, actor.Id, post.Id, "actor").Value;
        context.Notifications.Add(notif);
        await context.SaveChangesAsync();

        var result = await repo.GetByRecipientAsync(recipient.Id);

        result.Should().HaveCount(1);
        result.First().Message.Should().Contain("actor");
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsLimited()
    {
        var context = CreateContext();
        var repo = new NotificationRepository(context);
        var actor = await SeedUserAsync(context, "actor");
        var recipient = await SeedUserAsync(context, "recip");

        var post = Post.Create(recipient.Id, "P", PostContentType.Image, "/i.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            context.Notifications.Add(
                Notification.CreateComment(recipient.Id, actor.Id, post.Id, "actor").Value
            );
        }
        await context.SaveChangesAsync();

        var result = await repo.GetRecentAsync(recipient.Id, 3);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCorrectCount()
    {
        var context = CreateContext();
        var repo = new NotificationRepository(context);
        var actor = await SeedUserAsync(context, "actor");
        var recipient = await SeedUserAsync(context, "recip");

        var post = Post.Create(recipient.Id, "P", PostContentType.Image, "/i.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var n1 = Notification.CreateComment(recipient.Id, actor.Id, post.Id, "actor").Value;
        var n2 = Notification.CreateComment(recipient.Id, actor.Id, post.Id, "actor").Value;
        context.Notifications.AddRange(n1, n2);
        await context.SaveChangesAsync();
        n1.MarkAsRead();
        await context.SaveChangesAsync();

        var unread = await repo.GetUnreadCountAsync(recipient.Id);

        unread.Should().Be(1);
    }

    [Fact]
    public async Task MarkAsReadAsync_UpdatesStatus()
    {
        var context = CreateContext();
        var repo = new NotificationRepository(context);
        var actor = await SeedUserAsync(context, "actor");
        var recipient = await SeedUserAsync(context, "recip");

        var post = Post.Create(recipient.Id, "P", PostContentType.Image, "/i.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var notif = Notification.CreateComment(recipient.Id, actor.Id, post.Id, "actor").Value;
        context.Notifications.Add(notif);
        await context.SaveChangesAsync();

        await repo.MarkAsReadAsync(notif.Id, recipient.Id);
        await context.SaveChangesAsync();

        var retrieved = await repo.GetForRecipientAsync(notif.Id, recipient.Id);
        retrieved!.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAllAsReadAsync_MarksAllUnread()
    {
        var context = CreateContext();
        var repo = new NotificationRepository(context);
        var actor = await SeedUserAsync(context, "actor");
        var recipient = await SeedUserAsync(context, "recip");

        var post = Post.Create(recipient.Id, "P", PostContentType.Image, "/i.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        for (int i = 0; i < 3; i++)
        {
            context.Notifications.Add(
                Notification.CreateComment(recipient.Id, actor.Id, post.Id, "actor").Value
            );
        }
        await context.SaveChangesAsync();

        await repo.MarkAllAsReadAsync(recipient.Id);
        await context.SaveChangesAsync();

        var unread = await repo.GetUnreadCountAsync(recipient.Id);
        unread.Should().Be(0);
    }

    [Fact]
    public async Task GetForRecipientAsync_WrongRecipient_ReturnsNull()
    {
        var context = CreateContext();
        var repo = new NotificationRepository(context);
        var actor = await SeedUserAsync(context, "actor");
        var recipient = await SeedUserAsync(context, "recip");
        var other = await SeedUserAsync(context, "other", "other@t.com");

        var post = Post.Create(recipient.Id, "P", PostContentType.Image, "/i.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var notif = Notification.CreateComment(recipient.Id, actor.Id, post.Id, "actor").Value;
        context.Notifications.Add(notif);
        await context.SaveChangesAsync();

        var result = await repo.GetForRecipientAsync(notif.Id, other.Id);

        result.Should().BeNull();
    }
}
