namespace Kitakun.VkModules.Core.Domain
{
	using System;
    using System.ComponentModel.DataAnnotations;

    public class Subscription : IEntity
	{
		public int Id { get; set; }

        [Display(Name = "UserId - Vk ID пользователя, кто сможет управлять группой")]
		public long UserId { get; set; }

        [Display(Name = "GroupId - Vk ID группы или сообщества, которую будем считать")]
        public long? GroupId { get; set; }

        [Display(Name = "From - С какой даты будет доступен функционал")]
        public DateTime From { get; set; }

        [Display(Name = "To - Когда закончится именно эта подписка")]
        public DateTime? To { get; set; }
	}
}
