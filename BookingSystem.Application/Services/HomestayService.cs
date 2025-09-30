using BookingSystem.Application.Contracts;
using BookingSystem.Domain.Base;
using AutoMapper;
using BookingSystem.Domain.Repositories;
using Microsoft.Extensions.Logging;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Domain.Exceptions;

namespace BookingSystem.Application.Services
{
    public class HomestayService : IHomestayService
    {
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHomestayRepository _homestayRepository;
		private readonly ILogger<HomestayService> _logger;
		private readonly ICloudinaryService _cloudinaryService;
		private readonly IPropertyTypeRepository _propertyTypeRepository;
		public HomestayService(IUnitOfWork unitOfWork,
								IMapper mapper,
								IHomestayRepository homestayRepository,
								ILogger<HomestayService> logger,
								ICloudinaryService cloudinaryService,
								IPropertyTypeRepository propertyTypeRepository
		)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_homestayRepository = homestayRepository;
			_logger = logger;
			_cloudinaryService = cloudinaryService;
			_propertyTypeRepository = propertyTypeRepository;
		}

		public async Task<HomestayDto?> CreateAsync(CreateHomestayDto request)
		{
			var uploadedPublicIds = new List<string>();
			try
			{
				_logger.LogInformation("Starting homestay creation process.");

				var existingPropertyType = await _propertyTypeRepository.GetByIdAsync(request.PropertyTypeId);

				if (existingPropertyType == null)
				{
					_logger.LogWarning("Property type with ID {PropertyTypeId} not found.", request.PropertyTypeId);
					throw new NotFoundException($"Property type with ID {request.PropertyTypeId} not found.");
				}

				var existingOwner = await _unitOfWork.UserRepository.GetByIdAsync(request.OwnerId);

				if (existingOwner == null)
				{
					_logger.LogWarning("Owner with ID {OwnerId} not found.", request.OwnerId);
					throw new NotFoundException($"Owner with ID {request.OwnerId} not found.");
				}

			} catch (Exception ex)
			{
				throw;
			}
		}

	}
}
