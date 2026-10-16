# Add

#ifdef __AUTO_HUNT__
#include "PythonPlayer.h"
#endif

# Search

	STATEMANAGER.SaveTextureStageState(0, D3DTSS_MINFILTER, D3DTEXF_LINEAR);
	STATEMANAGER.SaveTextureStageState(0, D3DTSS_MAGFILTER, D3DTEXF_LINEAR);

# Add after

#ifdef __AUTO_HUNT__
	if (m_bAutoHuntRageStatus && !m_MiniMapAutoHuntRange.IsEmpty())
	{
		float calc = (1.0 / 100.0) * (m_fAutoHuntImageRadius * (m_fScale / 2.0));
		if (calc > 1.2)
			calc = 1.2;
		m_MiniMapAutoHuntRange.SetScale(calc, calc);
		const float newRadius = calc * (100.0 / 1.0);
		m_MiniMapAutoHuntRange.SetPosition((m_fWidth - newRadius) / 2.0f + m_fScreenX, (m_fHeight - newRadius) / 2.0f + m_fScreenY);
		m_MiniMapAutoHuntRange.SetDiffuseColor(1.0, 1.0, 1.0, 0.2);
		m_MiniMapAutoHuntRange.Render();
	}
	if (CPythonPlayer::Instance().GetAutoHuntStatus() && !m_MiniMapAutoHuntStartPoint.IsEmpty())
	{
		const TPixelPosition* m_bAutoHuntPosition = CPythonPlayer::Instance().GetAutoHuntStartPosition();

		const float calc = (1.0 / 15.0) * (15.0 * (m_fScale / 2.0));
		m_MiniMapAutoHuntStartPoint.SetScale(calc, calc);
	
		const float fooCellScale = 1.0f / ((float)CTerrainImpl::CELLSCALE);
	
		const float fDistanceFromCenterX = (m_bAutoHuntPosition->x - m_fCenterX) * fooCellScale * m_fScale;
		const float fDistanceFromCenterY = (m_bAutoHuntPosition->y - m_fCenterY) * fooCellScale * m_fScale;
	
		const float newRadius = calc * (15.0 / 1.0);
	
		m_MiniMapAutoHuntStartPoint.SetPosition((m_fWidth - newRadius) / 2.0f + fDistanceFromCenterX + m_fScreenX, (m_fHeight - newRadius) / 2.0f + fDistanceFromCenterY + m_fScreenY);
		m_MiniMapAutoHuntStartPoint.Render();
	}
#endif

# Search

	pImage = (CGraphicImage*)CResourceManager::Instance().GetResourcePointer(strImageCamera.c_str());
	m_MiniMapCameraraphicImageInstance.SetImagePointer(pImage);

# ADd after

#ifdef __AUTO_HUNT__
	const std::string strImageRange = strImageRoot + "minimap_range.tga";
	pImage = (CGraphicImage*)CResourceManager::Instance().GetResourcePointer(strImageRange.c_str());
	m_MiniMapAutoHuntRange.SetImagePointer(pImage);

	const std::string strImageStar = strImageRoot + "minimap/star.tga";
	pImage = (CGraphicImage*)CResourceManager::Instance().GetResourcePointer(strImageStar.c_str());
	m_MiniMapAutoHuntStartPoint.SetImagePointer(pImage);

	m_bAutoHuntRageStatus = false;
	m_fAutoHuntImageRadius = pImage->GetWidth();
#endif

# Search

	m_MiniMapCameraraphicImageInstance.Destroy();

# ADd after

#ifdef __AUTO_HUNT__
	if(!m_MiniMapAutoHuntRange.IsEmpty())
		m_MiniMapAutoHuntRange.Destroy();
	if(!m_MiniMapAutoHuntStartPoint.IsEmpty())
		m_MiniMapAutoHuntStartPoint.Destroy();
#endif

# Add

#ifdef __AUTO_HUNT__
void CPythonMiniMap::ChangeAutoHuntRange(float fVal)
{
	m_fAutoHuntImageRadius = fVal + 40.0;
	__SetPosition();
}
#endif


