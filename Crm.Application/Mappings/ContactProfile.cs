using AutoMapper;
using Crm.Application.DTOs.DynamicEntities;
using Crm.Application.DTOs.DynamicRecords;
using Crm.Application.DTOs.DynamicFieldValues;
using Crm.Application.DTOs.DynamicUi;
using Crm.Domain.Entities;
using System;
using System.Collections.Generic;
using Crm.Application.DOTs.Contact;
using Crm.Domain.Entities.Dynamic;

namespace Crm.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // -------------------- CONTACT MAPPINGS --------------------

            CreateMap<Contact, ContactDto>().ReverseMap();

            CreateMap<CreateContactDto, Contact>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore());

            CreateMap<UpdateContactDto, Contact>()
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedByUserId, opt => opt.Ignore());

            // -------------------- DYNAMIC ENTITY MAPPINGS --------------------

            CreateMap<DynamicEntity, DynamicEntityDto>()
                .ForMember(dest => dest.Fields, opt => opt.MapFrom(src => src.Fields))
                .ForMember(dest => dest.Fields, opt => opt.NullSubstitute(new List<DynamicFieldDto>()));

            CreateMap<DynamicField, DynamicFieldDto>().ReverseMap();

            CreateMap<CreateDynamicEntityDto, DynamicEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Fields, opt => opt.MapFrom(src => src.Fields))
                .ForMember(dest => dest.Fields, opt => opt.NullSubstitute(new List<CreateDynamicFieldDto>()));

            CreateMap<CreateDynamicFieldDto, DynamicField>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EntityId, opt => opt.Ignore());

            CreateMap<UpdateDynamicEntityDto, DynamicEntity>()
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Fields, opt => opt.MapFrom(src => src.Fields))
                .ForMember(dest => dest.Fields, opt => opt.NullSubstitute(new List<UpdateDynamicFieldDto>()));

            CreateMap<UpdateDynamicFieldDto, DynamicField>()
                .ForMember(dest => dest.EntityId, opt => opt.Ignore());

            // -------------------- DYNAMIC RECORD MAPPINGS --------------------

            CreateMap<DynamicRecord, DynamicRecordDto>()
                .ForMember(dest => dest.EntityName, opt => opt.MapFrom(src => src.Entity != null ? src.Entity.Name : string.Empty))
                .ReverseMap();

            CreateMap<CreateDynamicRecordDto, DynamicRecord>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RecordKey, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.FieldValues, opt => opt.MapFrom(src => src.FieldValues));

            CreateMap<UpdateDynamicRecordDto, DynamicRecord>()
                .ForMember(dest => dest.FieldValues, opt => opt.Ignore());

            // -------------------- DYNAMIC FIELD VALUE MAPPINGS --------------------

            CreateMap<DynamicFieldValue, DynamicFieldValueDto>()
                .ForMember(dest => dest.FieldLabel, opt => opt.MapFrom(src => src.Field != null ? src.Field.Label : string.Empty))
                .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.Field != null ? src.Field.EntityId : Guid.Empty))
                .ReverseMap();

            CreateMap<CreateDynamicFieldValueDto, DynamicFieldValue>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Record, opt => opt.Ignore())
                .ForMember(dest => dest.Field, opt => opt.Ignore());

            CreateMap<UpdateDynamicFieldValueDto, DynamicFieldValue>()
                .ForMember(dest => dest.Record, opt => opt.Ignore())
                .ForMember(dest => dest.Field, opt => opt.Ignore());

            // -------------------- DYNAMIC RELATIONSHIP MAPPINGS --------------------

            CreateMap<DynamicRelationship, CreateDynamicRelationshipDto>().ReverseMap();
            CreateMap<DynamicRelationship, UpdateDynamicRelationshipDto>().ReverseMap();
            CreateMap<DynamicRelationship, DynamicRelationshipDto>().ReverseMap();
        }
    }
}
