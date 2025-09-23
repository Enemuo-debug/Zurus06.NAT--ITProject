using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.dtos;
using server.NATModels;

namespace server.MappersAndExtensions
{
    public class JointMapper
    {
        public static UserDetailsDto MapToUserDetailsDto(OutputUserDto userDetails, List<OutputPostDto> userPosts)
        {
            return new UserDetailsDto
            {
                UserDetails = userDetails,
                UserPosts = userPosts
            };
        }

        public static bool ComparePosts(NATPosts post1, NATPosts post2)
        {
            return post1.Content == post2.Content && post1.Caption == post2.Caption && post1.Intro == post2.Intro && post1.userId == post2.userId && post1.Id == post2.Id;
        }
    }
}