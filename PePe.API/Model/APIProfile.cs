using AutoMapper;
using PePe.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PePe.API.Model {
    public class APIProfile : Profile {
        public APIProfile() {
            CreateMap<Menu, PePeMenu>();
        }
    }
}
